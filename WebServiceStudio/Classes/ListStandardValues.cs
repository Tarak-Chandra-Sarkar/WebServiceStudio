using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace WebServiceStudio
{
    internal class ListStandardValues : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                Array possibleValues = this.GetPossibleValues(context);
                if (possibleValues != null)
                {
                    foreach (object obj2 in possibleValues)
                    {
                        if (obj2.ToString() == value.ToString())
                        {
                            return obj2;
                        }
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        protected virtual Array GetPossibleValues(ITypeDescriptorContext context)
        {
            MethodInfo method = context.Instance.GetType().GetMethod("Get" + context.PropertyDescriptor.Name + "List");
            if (method == null)
            {
                return null;
            }
            return (method.Invoke(context.Instance, null) as Array);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            Array possibleValues = this.GetPossibleValues(context);
            return new TypeConverter.StandardValuesCollection((possibleValues != null) ? ((ICollection) possibleValues) : ((ICollection) new object[0]));
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

