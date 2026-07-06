////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class SleekAssetField : SleekWrapper
	{
		private System.Type _expectedType = typeof(Asset);
		public System.Type ExpectedType
		{
			get => _expectedType;
			set
			{
				_expectedType = value;
				UpdateInfoBox();
			}
		}

		private CachingAssetRef _value;
		public CachingAssetRef Value
		{
			get => _value;
			set
			{
				_value = value;
				SynchronizeField();
				UpdateInfoBox();
			}
		}

		public string TooltipText
		{
			get => infoBox.TooltipText;
			set
			{
				idField.TooltipText = value;
				infoBox.TooltipText = value;
			}
		}

		public event System.Action<SleekAssetField> OnValueChanged;

		public SleekAssetField(CachingAssetRef value, System.Type expectedType)
		{
			SizeOffset_Y = 60;

			_value = value;
			_expectedType = expectedType;

			idField = Glazier.Get().CreateStringField();
			idField.SizeScale_X = 1.0f;
			idField.SizeScale_Y = 0.5f;
			idField.OnTextChanged += OnTextChanged;
			idField.OnTextSubmitted += OnTextSubmitted;
			AddChild(idField);

			infoBox = Glazier.Get().CreateBox();
			infoBox.PositionScale_Y = 0.5f;
			infoBox.SizeScale_X = 1.0f;
			infoBox.SizeScale_Y = 0.5f;
			AddChild(infoBox);

			SynchronizeField();
			UpdateInfoBox();
		}

		public SleekAssetField(System.Type expectedType)
			: this(null, expectedType)
		{ }

		public SleekAssetField()
			: this(null, typeof(Asset))
		{ }

		private void OnTextChanged(ISleekField field, string value)
		{
			UpdateValue();
		}

		private void OnTextSubmitted(ISleekField field)
		{
			UpdateValue();
		}

		private void UpdateValue()
		{
			CachingAssetRef.TryParse(idField.Text, out CachingAssetRef newValue);
			if (_value != newValue)
			{
				_value = newValue;
				UpdateInfoBox();
				OnValueChanged?.Invoke(this);
			}
		}

		private void SynchronizeField()
		{
			idField.Text = _value.Guid.ToString("N");
		}

		private void UpdateInfoBox()
		{
			Asset asset = _value.Get();
			if (asset == null)
			{
				infoBox.TextColor = ESleekTint.FONT;
				infoBox.Text = "null";
			}
			else
			{
				System.Type actualType = asset.GetType();
				if (_expectedType.IsAssignableFrom(actualType))
				{
					infoBox.TextColor = ESleekTint.FONT;
				}
				else
				{
					infoBox.TextColor = ESleekTint.BAD;
				}
				infoBox.Text = asset.FriendlyName;
			}
		}

		private ISleekField idField;
		private ISleekBox infoBox;
	}
}
