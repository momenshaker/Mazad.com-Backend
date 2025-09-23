using System.Reflection;

namespace Mazad.Application.Common;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
