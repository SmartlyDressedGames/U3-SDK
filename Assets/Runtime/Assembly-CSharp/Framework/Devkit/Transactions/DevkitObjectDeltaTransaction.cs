////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define LOG_OBJECT_DELTA

using SDG.Framework.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SDG.Framework.Devkit.Transactions
{
	public class DevkitObjectDeltaTransaction : IDevkitTransaction
	{
		protected object instance;
		protected List<object> tempFields;
		protected List<object> tempProperties;
		protected List<ITransactionDelta> deltas;

		public bool delta => deltas.Count > 0;

		public void undo()
		{
			for (int index = 0; index < deltas.Count; index++)
			{
				deltas[index].undo(instance);
			}
		}

		public void redo()
		{
			for (int index = 0; index < deltas.Count; index++)
			{
				deltas[index].redo(instance);
			}
		}

		public void begin()
		{
			tempFields = ListPool<object>.claim();
			tempProperties = ListPool<object>.claim();

			Type type = instance.GetType();

			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			for (int fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
			{
				try
				{
					FieldInfo field = fields[fieldIndex];
					object value = field.GetValue(instance);
					tempFields.Add(value);
				}
				catch
				{
					tempFields.Add(null);
				}
			}

			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
			{
				try
				{
					PropertyInfo property = properties[propertyIndex];
					if (property.CanRead && property.CanWrite)
					{
						object value = property.GetValue(instance, null);
						tempProperties.Add(value);
					}
					else
					{
						tempProperties.Add(null);
					}
				}
				catch
				{
					tempProperties.Add(null);
				}
			}
		}

		public void end()
		{
			deltas = ListPool<ITransactionDelta>.claim();

			Type type = instance.GetType();

			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			for (int fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
			{
				try
				{
					FieldInfo field = fields[fieldIndex];
					object value = field.GetValue(instance);

					if (changed(tempFields[fieldIndex], value))
					{
#if LOG_OBJECT_DELTA
						SDG.Unturned.UnturnedLog.info(type.Name + " field " + field.Name + " changed from " + tempFields[fieldIndex] + " to " + value);
#endif
						deltas.Add(new TransactionFieldDelta(field, tempFields[fieldIndex], value));
					}
				}
				catch (Exception exception)
				{
					SDG.Unturned.UnturnedLog.exception(exception);
				}
			}

			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
			{
				try
				{
					PropertyInfo property = properties[propertyIndex];
					if (property.CanRead && property.CanWrite)
					{
						object value = property.GetValue(instance, null);

						if (changed(tempProperties[propertyIndex], value))
						{
#if LOG_OBJECT_DELTA
							SDG.Unturned.UnturnedLog.info(type.Name + " property " + property.Name + " changed from " + tempProperties[propertyIndex] + " to " + value);
#endif
							deltas.Add(new TransactionPropertyDelta(property, tempProperties[propertyIndex], value));
						}
					}
				}
				catch (Exception exception)
				{
					SDG.Unturned.UnturnedLog.exception(exception);
				}
			}

			ListPool<object>.release(tempFields);
			ListPool<object>.release(tempProperties);
		}

		public void forget()
		{
			if (deltas != null)
			{
				ListPool<ITransactionDelta>.release(deltas);
				deltas = null;
			}
		}

		protected bool changed(object before, object after)
		{
			if (before == null || after == null)
			{
				return before != after;
			}
			else
			{
				return !before.Equals(after);
			}
		}

		public DevkitObjectDeltaTransaction(object newInstance)
		{
			instance = newInstance;
		}
	}
}
