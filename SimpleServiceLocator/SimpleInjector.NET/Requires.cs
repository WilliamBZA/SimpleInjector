﻿#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector
{
    using System;
    using System.Linq;

    internal static class Requires
    {
        private static readonly Type[] AmbiguousTypes = new[] { typeof(Type), typeof(string) };

        internal static void IsNotNull(object instance, string paramName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        internal static void IsReferenceType(Type type, string paramName)
        {
            if (!type.IsClass && !type.IsInterface)
            {
                throw new ArgumentException(StringResources.SuppliedTypeIsNotAReferenceType(type), paramName);
            }
        }

        internal static void IsNotOpenGenericType(Type type, string paramName)
        {
            // We check for ContainsGenericParameters to see whether there is a Generic Parameter 
            // to find out if this type can be created.
            if (type.ContainsGenericParameters)
            {
                throw new ArgumentException(StringResources.SuppliedTypeIsAnOpenGenericType(type), paramName);
            }
        }

        internal static void ServiceIsAssignableFromImplementation(Type service, Type implementation,
            string paramName)
        {
            if (!service.IsAssignableFrom(implementation))
            {
                throw new ArgumentException(
                    StringResources.SuppliedTypeDoesNotInheritFromOrImplement(service, implementation),
                    paramName);
            }
        }

        internal static void IsNotAnAmbiguousType(Type type, string paramName)
        {
            if (AmbiguousTypes.Contains(type))
            {
                throw new ArgumentException(StringResources.TypeIsAmbiguous(type), paramName);
            }
        }
    }
}