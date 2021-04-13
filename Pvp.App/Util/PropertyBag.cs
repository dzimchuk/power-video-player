/* ****************************************************************************
 *
 * Copyright (c) Andrei Dzimchuk. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pvp.App.Util
{
    internal class PropertyBag
    {
        private readonly IDictionary<string, object> _props;
         
        public PropertyBag() // used when saving properties
        {
            _props = new Dictionary<string, object>();
        }

        public PropertyBag(Stream stream) // used to load properties
        {
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                _props = (IDictionary<string, object>)formatter.Deserialize(stream);
            }
            catch (SerializationException)
            {
                _props = new Dictionary<string, object>();
            }
        }

        public void Save(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, _props);
        }

        public void Set<T>(string name, T value)
        {
            if (_props.ContainsKey(name))
                _props[name] = value;
            else
                _props.Add(name, value);
        }

        public T Get<T>(string name, T defaultValue)
        {
            object value;
            if (_props.TryGetValue(name, out value))
            {
                if (value.GetType() != typeof(T))
                    value = defaultValue;
            }
            else
                value = defaultValue;
            return (T)value;
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            bool bRet = false;
            object o;
            if (_props.TryGetValue(name, out o) && o.GetType() == typeof(T))
            {
                value = (T)o;
                bRet = true;
            }
            else
            {
                value = default(T);
            }

            return bRet;
        }
    }
}
