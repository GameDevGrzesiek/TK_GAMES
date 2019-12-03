using System;

public enum InjectFrom
{
    Above,
    Anywhere
}

public class InjectAttribute : Attribute
{
    public InjectFrom InjectFrom { get; private set; }

    public InjectAttribute(InjectFrom injectFrom)
    {
        this.InjectFrom = injectFrom;
    }
}

