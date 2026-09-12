using Godot;
using System;

public partial class ImpaleComponent : Node
{
	private static readonly PackedScene _pickableItemScence = GD.Load<PackedScene>("res://scenes/equipment/pickable_item.tscn");
	private static readonly PackedScene _equipedItemScence = GD.Load<PackedScene>("res://scenes/equipment/equiped_item.tscn");

	[Signal] public delegate void ImpaledEventHandler(Vector3 impulse);

	[Export] public Node3D ImpalePlaceholder;
	[Export] public Vector3 OffsetPosition; //武器相对初始化位置的局部偏移


	public void Impale(ThrowableItem throwableItem)
	{
		if (ImpalePlaceholder == null || throwableItem.EquipmentData == null)
		{
			return;
		}
		if (throwableItem.EquipmentData is WeaponData weaponData)
		{
			GameEvents.Instance.TriggerImpactFelt(GameEvents.ImpactIntensity.MEDIUM);
			var pickableItem = _equipedItemScence.Instantiate<EquipedItem>();
			pickableItem.EquipmentData = weaponData;
			ImpalePlaceholder.AddChild(pickableItem);
			pickableItem.GlobalBasis = throwableItem.OriginalBasis;
			pickableItem.TranslateObjectLocal(weaponData.ImpaleLocalTranslation);
			pickableItem.RotateObjectLocal(Vector3.Up, weaponData.ImpaleLocalRotation);
			EmitSignalImpaled(throwableItem.OriginalBasis * Vector3.Forward * 20 + Vector3.Up * 20); //等待布娃娃物理启用
		}

	}

}

public partial interface IImpaleable
{
	public ImpaleComponent ImpaleComponent {get; set;}
	bool CanImpale(WeaponData weaponData);
	void Impale(ThrowableItem throwableItem);
}