﻿#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
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

namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.Linq.Expressions;

    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    public class PerWcfOperationLifestyle : Lifestyle
    {
        internal static readonly PerWcfOperationLifestyle WithDisposal = new PerWcfOperationLifestyle(true);
        internal static readonly PerWcfOperationLifestyle NoDisposal = new PerWcfOperationLifestyle(false);

        private readonly bool disposeInstanceWhenOperationEnds;

        public PerWcfOperationLifestyle(bool disposeInstanceWhenOperationEnds = true)
        {
            this.disposeInstanceWhenOperationEnds = disposeInstanceWhenOperationEnds;
        }

        public override LifestyleRegistration CreateRegistration<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            throw new NotImplementedException();
        }

        public override LifestyleRegistration CreateRegistration<TService, TImplementation>(Container container)
        {
            throw new NotImplementedException();
        }

        protected override void OnRegistration(LifestyleRegistrationEventArgs e)
        {
            SimpleInjectorWcfExtensions.EnablePerWcfOperationLifestyle(e.Container);
        }

        private sealed class PerWcfOperationLifestyleRegistration<TService>
            : PerWcfOperationLifestyleRegistration<TService, TService>
            where TService : class
        {
            internal PerWcfOperationLifestyleRegistration(Container container)
                : base(container)
            {
            }

            public Func<TService> InstanceCreator { get; set; }

            protected override Func<TService> BuildInstanceCreator()
            {
                return this.BuildTransientDelegate(this.InstanceCreator);
            }
        }

        private class PerWcfOperationLifestyleRegistration<TService, TImplementation> : LifestyleRegistration
            where TService : class
            where TImplementation : class, TService
        {
            private Func<TService> instanceCreator;
            private WcfOperationScopeManager manager;

            internal PerWcfOperationLifestyleRegistration(Container container)
                : base(container)
            {
            }

            public bool Dispose { get; set; }

            public override Expression BuildExpression()
            {
                if (this.instanceCreator == null)
                {
                    this.manager = this.Container.GetInstance<WcfOperationScopeManager>();

                    this.instanceCreator = this.BuildInstanceCreator();
                }

                return Expression.Call(Expression.Constant(this), this.GetType().GetMethod("GetInstance"));
            }

            // This method needs to be public, because the BuildExpression methods build a
            // MethodCallExpression using this method, and this would fail in partial trust when the 
            // method is not public.
            public TService GetInstance()
            {
                var scope = this.manager.CurrentScope;

                if (scope == null)
                {
                    return this.GetInstanceWithoutScope();
                }

                return scope.GetInstance(this.instanceCreator, this.Dispose);
            }

            protected virtual Func<TService> BuildInstanceCreator()
            {
                return this.BuildTransientDelegate<TService, TImplementation>();
            }

            private TService GetInstanceWithoutScope()
            {
                if (this.Container.IsVerifying())
                {
                    // Return a transient instance when this method is called during verification
                    return this.instanceCreator();
                }

                throw new ActivationException("The " + typeof(TService).Name + " is registered as " +
                    "'PerWcfOperation', but the instance is requested outside the context of a WCF " +
                    "operation.");
            }
        }
    }
}
