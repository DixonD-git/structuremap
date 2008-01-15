using System;
using StructureMap.Graph;

namespace StructureMap.Configuration.Tokens.Properties
{
    public enum ChildPropertyType
    {
        InlineDefinition,
        Reference,
        Default,
        NotDefined
    }

    [Serializable]
    public class ChildProperty : Property
    {
        private int _arrayIndex = -1;
        private ChildPropertyType _childType = ChildPropertyType.NotDefined;
        private InstanceToken _innerInstance;
        private IChildPropertyMode _mode;
        private Type _pluginType;
        private string _pluginTypeName;
        private string _referenceKey = string.Empty;

        protected ChildProperty(PropertyDefinition definition) : base(definition)
        {
            _mode = new NulloChildPropertyMode(this);
        }

        public ChildProperty(PropertyDefinition definition, InstanceMemento memento, PluginGraphReport report)
            : this(definition)
        {
            InstanceMemento propertyMemento = memento.GetChildMemento(definition.PropertyName);
            initialize(definition, propertyMemento, report);
        }

        public override string PropertyName
        {
            get
            {
                if (ArrayIndex >= 0)
                {
                    return base.PropertyName + " #" + ArrayIndex;
                }
                return base.PropertyName;
            }
        }

        public int ArrayIndex
        {
            get { return _arrayIndex; }
            set { _arrayIndex = value; }
        }

        public ChildPropertyType ChildType
        {
            get { return _childType; }
            set { _childType = value; }
        }

        public InstanceToken InnerInstance
        {
            get { return _innerInstance; }
            set { _innerInstance = value; }
        }

        public string ReferenceKey
        {
            get { return _referenceKey; }
            set { _referenceKey = value; }
        }

        public Type PluginType
        {
            get { return _pluginType; }
            set { _pluginType = value; }
        }

        public override GraphObject[] Children
        {
            get
            {
                if (_innerInstance == null)
                {
                    return new GraphObject[0];
                }
                else
                {
                    return new GraphObject[] {_innerInstance};
                }
            }
        }

        public string PluginTypeName
        {
            get { return _pluginTypeName; }
            set { _pluginTypeName = value; }
        }

        public static ChildProperty BuildArrayChild(PropertyDefinition definition, InstanceMemento memento,
                                                    PluginGraphReport report)
        {
            ChildProperty child = new ChildProperty(definition);
            child.initialize(definition, memento, report);

            return child;
        }

        private void initialize(PropertyDefinition definition, InstanceMemento propertyMemento, PluginGraphReport report)
        {
            _pluginType = definition.PropertyType;
            _pluginTypeName = TypePath.GetAssemblyQualifiedName(_pluginType);

            if (propertyMemento == null)
            {
                // Assume that the reference is to the default of the property's type
                _childType = ChildPropertyType.Default;
                _mode = new DefaultChildPropertyMode(this);
            }
            else if (propertyMemento.IsDefault)
            {
                _childType = ChildPropertyType.Default;
                _mode = new DefaultChildPropertyMode(this);
            }
            else if (propertyMemento.IsReference)
            {
                _childType = ChildPropertyType.Reference;
                _referenceKey = propertyMemento.ReferenceKey;
                _mode = new ReferencedChildPropertyMode(this);
            }
            else
            {
                _childType = ChildPropertyType.InlineDefinition;
                Type propertyType = definition.PropertyType;
                _innerInstance = new InstanceToken(propertyType, report, propertyMemento);
                _mode = new InlineInstanceChildPropertyMode(this);
            }
        }

        public override void Validate(IInstanceValidator validator)
        {
            _mode.Validate(validator);
        }


        public override void AcceptVisitor(IConfigurationVisitor visitor)
        {
            _mode.AcceptVisitor(visitor);
        }
    }
}