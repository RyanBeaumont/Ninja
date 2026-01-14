using UnityEngine;
public class AnimationListener : MonoBehaviour
{
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    void SpawnWeapon(string weapon)
    {
        //find WeaponR in skeleton (searches deep hierarchy)
        var weaponHolder = FindTransformRecursive(transform, "WeaponR");
        if(weaponHolder == null){print("WeaponR not found in skeleton");return;}
        foreach(Transform child in weaponHolder)
        {
            Destroy(child.gameObject);
        }
        var weaponPrefab = Resources.Load<GameObject>($"Weapons/{weapon}");
        if(weaponPrefab == null){print("Weapon model not found");return;}
        var i = Instantiate(weaponPrefab, weaponHolder);
        i.transform.localPosition = Vector3.zero;
        i.transform.localRotation = Quaternion.identity;
    }

    Transform FindTransformRecursive(Transform root, string name)
    {
        if (root.name == name)
            return root;
        
        foreach (Transform child in root)
        {
            var result = FindTransformRecursive(child, name);
            if (result != null)
                return result;
        }
        
        return null;
    }

    void Hit(string direction)
    {
        if(GetComponentInParent<EnemyCombatant>() != null)
        {
            GetComponentInParent<EnemyCombatant>().OnHit(direction);
        }
        else if(GetComponentInParent<PlayerCombatant>() != null)
        {
            GetComponentInParent<PlayerCombatant>().OnHit();
        }
    }

    void SlowMo()
    {
        if(GetComponentInParent<EnemyCombatant>() != null)
        {
            var effect = Instantiate(Resources.Load<GameObject>("Particles/HitLight"), transform);
            //Time.timeScale = 0.125f;
        }
    }
}
