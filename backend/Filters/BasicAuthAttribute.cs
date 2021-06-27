using Microsoft.AspNetCore.Mvc;
using System;

namespace backend.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BasicAuthAttribute : TypeFilterAttribute
    {
        public BasicAuthAttribute() : base(typeof(BasicAuthFilter))
        {
            Arguments = new object[] { };
        }
    }
}
