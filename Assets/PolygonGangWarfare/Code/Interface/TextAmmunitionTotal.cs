// Copyright 2021, Infima Games. All Rights Reserved.

using System.Globalization;

namespace InfimaGames.LowPolyShooterPack.Interface
{
    /// <summary>
    /// Total Ammunition Text.
    /// </summary>
    public class TextAmmunitionTotal : ElementText
    {
        #region METHODS
        
        /// <summary>
        /// Tick.
        /// </summary>
        protected override void Tick()
        {
            Weapon currentWeapon = equippedWeapon as Weapon;

            if (InventoryManager.Instance != null && currentWeapon != null)
            {
                ItemData ammoType = currentWeapon.GetAmmoType();

                if (ammoType != null)
                {
                    int totalInBag = InventoryManager.Instance.GetAmmoCount(ammoType);

                    textMesh.text = totalInBag.ToString(CultureInfo.InvariantCulture);
                    return;
                }
            }
        }
        
        #endregion
    }
}