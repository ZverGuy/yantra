﻿using System;
using System.Collections.Generic;
using System.Text;
using YantraJS.Core.Core.Storage;
using YantraJS.Extensions;

namespace YantraJS.Core.Core
{
    public class JSPrototype
    {
        public readonly JSObject @object;
        private bool dirty = true;

        private UInt32Map<(JSProperty property, JSPrototype owner)> properties;
        private UInt32Map<(JSProperty property, JSPrototype owner)> elements;
        private UInt32Map<(JSProperty property, JSPrototype owner)> symbols;

        internal JSPrototype(JSObject @object)
        {
            this.@object = @object;

            this.Build();
        }

        private void Build()
        {
            
            if (!this.dirty)
                return;
            lock (this)
            {
                if (!this.dirty)
                    return;
                properties = new UInt32Map<(JSProperty, JSPrototype)>();
                elements = new UInt32Map<(JSProperty, JSPrototype)>();
                symbols = new UInt32Map<(JSProperty, JSPrototype)>();

                Build(this);
                dirty = false;
            }
        }

        private void Build(JSPrototype target)
        {
            // first build the base class for correct inheritance...

            var @object = target.@object;

            // if(@object.prototypeChain)
            var @base = @object.prototypeChain?.@object;
            if (@base != null && @base.prototypeChain != null && @base.prototypeChain != this)
                this.Build(@base.prototypeChain);

            if (@object.prototypeChain == null)
                return;
            this.Build(@object.prototypeChain);

            @object.PropertyChanged += @object_PropertyChanged;
            ref var objectProperties = ref @object.GetOwnProperties(false);
            if (objectProperties.properties != null)
            {
                foreach(var ep in objectProperties.AllValues())
                {
                    if (!ep.Value.IsEmpty)
                    {
                        properties[ep.Key] = (ep.Value.ToNotReadOnly(),target);
                    }
                }
                //for (int i = 0; i < objectProperties.properties.Length; i++)
                //{
                //    ref var ep = ref objectProperties.properties[i];
                //    if (!ep.IsEmpty)
                //    {
                //        properties[ep.key.Key] = (ep.ToNotReadOnly(), target);
                //    }
                //}
            }

            ref var objectElements = ref @object.GetElements(false);
            if (!objectElements.IsNull)
            {
                foreach(var e in objectElements.AllValues())
                {
                    if (!e.Value.IsEmpty)
                    {
                        elements[e.Key] = (e.Value.ToNotReadOnly(), target);
                    }
                }
            }

            ref var objectSymbols = ref @object.GetSymbols();
            if(!objectSymbols.IsNull)
            {
                foreach(var e in objectSymbols.AllValues())
                {
                    if (!e.Value.IsEmpty)
                    {
                        symbols[e.Key] = (e.Value.ToNotReadOnly(), target);
                    }
                }
            }
        }

        private void @object_PropertyChanged(JSObject sender, (uint keyString, uint index, JSSymbol symbol) index)
        {
            dirty = false;
        }

        internal JSProperty GetInternalProperty(in KeyString name)
        {
            this.Build();
            var (p, owner) = properties[name.Key];
            return p;
        }

        internal JSProperty GetInternalProperty(uint name)
        {
            this.Build();
            return elements[name].property;
        }

        internal JSProperty GetInternalProperty(JSSymbol symbol)
        {
            this.Build();
            return symbols[symbol.Key.Key].property;
        }

        internal JSFunctionDelegate GetMethod(in KeyString key)
        {
            this.Build();
            var (p, _) = properties[key.Key];
            if(p.IsValue)
            {
                if (p.get != null)
                    return p.get.f;
            }
            if (p.IsProperty)
            {
                return p.get.f;
            }
            return null;
        }

        internal bool TryRemove(uint i, out JSProperty p)
        {
            if(elements.TryGetValue(i, out var ee))
            {
                var @object = ee.owner.@object;
                ref var elements = ref @object.GetElements(false);
                return elements.TryRemove(i, out p);
            }
            p = JSProperty.Empty;
            return false;
        }

        public JSValue this[in KeyString k]
        {
            get
            {
                var p = GetInternalProperty(k);
                return @object.GetValue(p);
            }
        }
    }

}