using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public class DependencyManager : Singleton<DependencyManager>
{
    protected override void OnAwake()
    {
        base.OnAwake();
        ResolveScene();
    }

    public void FindObjects(IEnumerable<GameObject> allGameObjects, List<MonoBehaviour> injectables)
    {
        foreach (var gameObject in allGameObjects)
        {
            foreach (var component in gameObject.GetComponents<MonoBehaviour>())
            {
                var componentType = component.GetType();
                var hasInjectableProperties = componentType.GetProperties()
                    .Where(IsMemberInjectable)
                    .Any();
                if (hasInjectableProperties)
                {
                    injectables.Add(component);
                }
                else
                {
                    var hasInjectableFields = componentType.GetFields()
                        .Where(IsMemberInjectable)
                        .Any();
                    if (hasInjectableFields)
                    {
                        injectables.Add(component);
                    }
                }

                if (componentType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(IsMemberInjectable)
                    .Any())
                {
                    Debug.LogError("Private properties should not be marked with [Inject] atttribute!", component);
                }

                if (componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(IsMemberInjectable)
                    .Any())
                {
                    Debug.LogError("Private fields should not be marked with [Inject] atttribute!", component);
                }
            }
        }
    }

    private bool IsMemberInjectable(MemberInfo member)
    {
        return member.GetCustomAttributes(true)
            .Where(attribute => attribute is InjectAttribute)
            .Count() > 0;
    }

    private IEnumerable<IInjectableMember> FindInjectableMembers(MonoBehaviour injectable)
    {
        var type = injectable.GetType();
        var injectableProperties = type.GetProperties()
            .Where(IsMemberInjectable)
            .Select(property => new InjectableProperty(property))
            .Cast<IInjectableMember>();

        var injectableFields = type.GetFields()
            .Where(IsMemberInjectable)
            .Select(field => new InjectableField(field))
            .Cast<IInjectableMember>();

        return injectableProperties.Concat(injectableFields);
    }

    private IEnumerable<GameObject> GetAncestors(GameObject fromGameObject)
    {
        for (var parent = fromGameObject.transform.parent; parent != null; parent = parent.parent)
        {
            yield return parent.gameObject;
        }
    }

    private IEnumerable<MonoBehaviour> FindMatchingDependendencies(Type injectionType, GameObject gameObject)
    {
        foreach (var component in gameObject.GetComponents<MonoBehaviour>())
        {
            if (injectionType.IsAssignableFrom(component.GetType()))
            {
                yield return component;
            }
        }
    }

    private MonoBehaviour FindMatchingDependency(Type injectionType, GameObject gameObject, MonoBehaviour injectable)
    {
        var matchingDependencies = FindMatchingDependendencies(injectionType, gameObject).ToArray();
        if (matchingDependencies.Length == 1)
            return matchingDependencies[0];

        if (matchingDependencies.Length == 0)
            return null;

        Debug.LogError(
            "Found multiple hierarchy dependencies that match injection type " + injectionType.Name + " to be injected into '" + injectable.name + "'. See following warnings.",
            injectable
        );

        foreach (var dependency in matchingDependencies)
            Debug.LogWarning("  Duplicate dependencies: '" + dependency.name + "'.", dependency);

        return null;
    }

    private MonoBehaviour FindDependencyInHierarchy(Type injectionType, MonoBehaviour injectable)
    {
        foreach (var ancestor in GetAncestors(injectable.gameObject))
        {
            var dependency = FindMatchingDependency(injectionType, ancestor, injectable);
            if (dependency != null)
                return dependency;
        }

        return null;
    }

    public interface IInjectableMember
    {
        void SetValue(object owner, object value);
        string Name { get; }
        Type MemberType { get; }
        string Category { get; }
        InjectFrom InjectFrom { get; }
    }

    public class InjectableProperty : IInjectableMember
    {
        private PropertyInfo propertyInfo;

        public InjectableProperty(PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
            var injectAttribute = propertyInfo.GetCustomAttributes(typeof(InjectAttribute), false)
                .Cast<InjectAttribute>()
                .Single();
            this.InjectFrom = injectAttribute.InjectFrom;
        }

        public void SetValue(object owner, object value)
        {
            propertyInfo.SetValue(owner, value, null);
        }

        public string Name
        {
            get { return propertyInfo.Name; }
        }

        public Type MemberType
        {
            get { return propertyInfo.PropertyType; }
        }

        public string Category
        {
            get { return "property"; }
        }

        public InjectFrom InjectFrom { get;private set; }
    }

    public class InjectableField : IInjectableMember
    {
        private FieldInfo fieldInfo;

        public InjectableField(FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
            var injectAttribute = fieldInfo.GetCustomAttributes(typeof(InjectAttribute), false)
                .Cast<InjectAttribute>()
                .Single();
            this.InjectFrom = injectAttribute.InjectFrom;
        }

        public void SetValue(object owner, object value)
        {
            fieldInfo.SetValue(owner, value);
        }

        public string Name
        {
            get { return fieldInfo.Name; }
        }

        public Type MemberType
        {
            get { return fieldInfo.FieldType; }
        }

        public string Category
        {
            get { return "field"; }
        }

        public InjectFrom InjectFrom { get; private set; }
    }

    private bool ResolveMemberDependencyFromHierarchy(MonoBehaviour injectable, IInjectableMember injectableMember)
    {
        var toInject = FindDependencyInHierarchy(injectableMember.MemberType, injectable);
        if (toInject != null)
        {
            try
            {
                Debug.Log("Injecting " + toInject.GetType().Name + " from hierarchy (GameObject: '" + toInject.gameObject.name + "') into " + injectable.GetType().Name + " at " + injectableMember.Category + " " + injectableMember.Name + " on GameObject '" + injectable.name + "'.", injectable);

                injectableMember.SetValue(injectable, toInject);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, injectable);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool ResolveMemberDependencyFromAnywhere(MonoBehaviour injectable, IInjectableMember injectableMember)
    {
        if (injectableMember.MemberType.IsArray)
            return ResolveArrayDependencyFromAnywhere(injectable, injectableMember);
        else
            return ResolveObjectDependencyFromAnywhere(injectable, injectableMember);
    }

    private static bool ResolveArrayDependencyFromAnywhere(MonoBehaviour injectable, IInjectableMember injectableMember)
    {
        var elementType = injectableMember.MemberType.GetElementType();
        var toInject = GameObject.FindObjectsOfType(elementType);
        if (toInject != null)
        {
            try
            {
                Debug.Log("Injecting array of " + toInject.Length + " elements into " + injectable.GetType().Name + " at " + injectableMember.Category + " " + injectableMember.Name + " on GameObject '" + injectable.name + "'.", injectable);

                foreach (var component in toInject.Cast<MonoBehaviour>())
                {
                    Debug.Log("> Injecting object " + component.GetType().Name + " (GameObject: '" + component.gameObject.name + "').", injectable);
                }

                var typedArray = Array.CreateInstance(elementType, toInject.Length);
                Array.Copy(toInject, typedArray, toInject.Length);

                injectableMember.SetValue(injectable, typedArray);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, injectable);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool ResolveObjectDependencyFromAnywhere(MonoBehaviour injectable, IInjectableMember injectableMember)
    {
        var toInject = (MonoBehaviour)GameObject.FindObjectOfType(injectableMember.MemberType);
        if (toInject != null)
        {
            try
            {
                Debug.Log("Injecting object " + toInject.GetType().Name + " (GameObject: '" + toInject.gameObject.name + "') into " + injectable.GetType().Name + " at " + injectableMember.Category + " " + injectableMember.Name + " on GameObject '" + injectable.name + "'.", injectable);
                injectableMember.SetValue(injectable, toInject);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, injectable);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private void ResolveMemberDependency(MonoBehaviour injectable, IInjectableMember injectableMember)
    {
        if (injectableMember.InjectFrom == InjectFrom.Above)
        {
            if (!ResolveMemberDependencyFromHierarchy(injectable, injectableMember))
            {
                Debug.LogError(
                    "Failed to resolve dependency for " + injectableMember.Category + ". Member: " + injectableMember.Name + ", MonoBehaviour: " + injectable.GetType().Name + ", GameObject: " + injectable.gameObject.name + "\r\n" +
                    "Failed to find a dependency that matches " + injectableMember.MemberType.Name + ".",
                    injectable
                );
            }
        }
        else if (injectableMember.InjectFrom == InjectFrom.Anywhere)
        {
            if (!ResolveMemberDependencyFromAnywhere(injectable, injectableMember))
            {
                Debug.LogError(
                    "Failed to resolve dependency for " + injectableMember.Category + ". Member: " + injectableMember.Name + ", MonoBehaviour: " + injectable.GetType().Name + ", GameObject: " + injectable.gameObject.name + "\r\n" +
                    "Failed to find a dependency that matches " + injectableMember.MemberType.Name + ".",
                    injectable
                );
            }
        }
        else
        {
            throw new ApplicationException("Unexpected use of InjectFrom enum: " + injectableMember.InjectFrom);
        }
    }

    private void ResolveDependencies(MonoBehaviour injectable)
    {
        var injectableProperties = FindInjectableMembers(injectable);
        foreach (var injectableMember in injectableProperties)
            ResolveMemberDependency(injectable, injectableMember);
    }

    public void ResolveScene()
    {
        var allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        Resolve(allGameObjects);
    }

    public void Resolve(GameObject parent)
    {
        var gameObjects = new GameObject[] { parent };
        Resolve(gameObjects);
    }

    public void Resolve(IEnumerable<GameObject> gameObjects)
    {
        var injectables = new List<MonoBehaviour>();
        FindObjects(gameObjects, injectables);

        foreach (var injectable in injectables)
            ResolveDependencies(injectable);
    }
}
