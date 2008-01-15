using System;
using System.Collections;
using System.Collections.Generic;
using StructureMap.Configuration.Tokens;
using StructureMap.Graph;
using StructureMap.Source;

namespace StructureMap
{
    /// <summary>
    /// Abstract class that is the supertype of all storage and retrieval mechanisms of
    /// InstanceMemento instances
    /// </summary>
    [PluginFamily]
    public abstract class MementoSource
    {
        private InstanceMemento _defaultMemento;
        private Dictionary<string, InstanceMemento> _externalMementos = new Dictionary<string, InstanceMemento>();
        private PluginFamily _family;

        protected MementoSource() : base()
        {
        }

        public InstanceMemento DefaultMemento
        {
            get { return _defaultMemento; }
            set { _defaultMemento = value; }
        }

        /// <summary>
        /// The type of MementoSource
        /// </summary>
        public virtual MementoSourceType SourceType
        {
            get { return MementoSourceType.External; }
        }

        /// <summary>
        /// String description of the MementoSource.  Used in the StructureMap-Client UI.
        /// </summary>
        public abstract string Description { get; }

        public PluginFamily Family
        {
            get { return _family; }
            set { _family = value; }
        }

        /// <summary>
        /// Retrieves the named InstanceMemento
        /// </summary>
        /// <param name="instanceKey">The instanceKey of the requested InstanceMemento</param>
        /// <returns></returns>
        public InstanceMemento GetMemento(string instanceKey)
        {
            InstanceMemento returnValue = null;

            if (_externalMementos.ContainsKey(instanceKey))
            {
                returnValue = _externalMementos[instanceKey];
            }
            else if (containsKey(instanceKey))
            {
                try
                {
                    returnValue = retrieveMemento(instanceKey);
                }
                catch (StructureMapException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new StructureMapException(203, ex, instanceKey);
                }
            }

            // A null returnvalue is valid
            return returnValue;
        }

        public void AddExternalMemento(InstanceMemento memento)
        {
            _externalMementos[memento.InstanceKey] = memento;
        }


        public void SetDefault(string instanceKey)
        {
            _defaultMemento = GetMemento(instanceKey);
        }

        public virtual InstanceMemento ResolveMemento(InstanceMemento memento)
        {
            InstanceMemento returnValue = memento;

            if (memento.IsDefault)
            {
                if (_defaultMemento == null)
                {
                    string pluginTypeName = Family == null ? "UNKNOWN" : Family.PluginTypeName;
                    throw new StructureMapException(202, pluginTypeName);
                }

                returnValue = _defaultMemento;
            }
            else if (memento.IsReference)
            {
                returnValue = GetMemento(memento.ReferenceKey);

                if (returnValue == null)
                {
                    throw new StructureMapException(200, memento.ReferenceKey, Family.PluginTypeName);
                }
            }

            return returnValue;
        }


        protected abstract InstanceMemento[] fetchInternalMementos();

        /// <summary>
        /// Retrieves an array of all InstanceMemento's stored by this MementoSource
        /// </summary>
        /// <returns></returns>
        public InstanceMemento[] GetAllMementos()
        {
            ArrayList list = new ArrayList();
            list.AddRange(fetchInternalMementos());
            list.AddRange(_externalMementos.Values);

            return (InstanceMemento[]) list.ToArray(typeof (InstanceMemento));
        }

        /// <summary>
        /// Template pattern method.  Determines if the MementoSource contains a definition for the
        /// requested instanceKey.
        /// </summary>
        /// <param name="instanceKey"></param>
        /// <returns></returns>
        protected internal abstract bool containsKey(string instanceKey);

        /// <summary>
        /// Template pattern method.  Retrieves an InstanceMemento for the instanceKey
        /// </summary>
        /// <param name="instanceKey"></param>
        /// <returns></returns>
        protected abstract InstanceMemento retrieveMemento(string instanceKey);

        public virtual TemplateToken[] GetAllTemplates()
        {
            return new TemplateToken[0];
        }
    }
}