using UnityEngine;
public class AnimationListener : MonoBehaviour
{
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    public void Particle(GameObject particle)
    {
            var p = Instantiate(particle, transform);
            p.transform.localPosition = Vector3.zero;
            Destroy(p,5);
    }

    void SpawnWeapon(string weapon)
    {
        if(GetComponent<CopyAnimator>() != null) return;
        //find WeaponR in skeleton (searches deep hierarchy)
        //if the parent or child has a DefaultPose component, get its weapon game model
        var defaultPose = GetComponentInParent<DefaultPose>();
        var weaponHolder = FindTransformRecursive(transform, "WeaponR");
        GameObject weaponGO = null;
        if(defaultPose != null && defaultPose.combatWeapon != null && weapon != "" && weapon != "portocom_phone")
        {
            weaponGO = defaultPose.combatWeapon;
        }
        else
        {
            if(weapon == "portocom_phone")
            {
                weaponHolder = FindTransformRecursive(transform, "WeaponL");
            }
            weaponGO = Resources.Load<GameObject>($"Weapons/{weapon}");
        }
        if(weapon == "Soda"){weaponGO = Resources.Load<GameObject>($"Weapons/Soda");}
        
        ClearWeapon();

        if(weaponGO == null){print("Weapon model not found");return;}
        var i = Instantiate(weaponGO, weaponHolder);
        i.transform.localPosition = Vector3.zero;
        i.transform.localRotation = Quaternion.identity;
    }

    public void ClearWeapon()
    {
        var weaponHolder = FindTransformRecursive(transform, "WeaponL");
         if(weaponHolder == null){print("WeaponL not found in skeleton");return;}
        foreach(Transform child in weaponHolder)
        {
            Destroy(child.gameObject);
        }
        weaponHolder = FindTransformRecursive(transform, "WeaponR");
         if(weaponHolder == null){print("WeaponR not found in skeleton");return;}
        foreach(Transform child in weaponHolder)
        {
            Destroy(child.gameObject);
        }
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

    void SpawnProjectile(string prefab)
    {
        if(GetComponent<CopyAnimator>() != null) return;
        var c = GetComponentInParent<Combatant>();
        if(c != null)
            BattleManager.Instance.SpawnProjectile(c, prefab);
    }

    void Hit(string direction)
    {
        if(GetComponent<CopyAnimator>() != null) return;
        if(GetComponentInParent<EnemyCombatant>() != null)
        {
            GetComponentInParent<EnemyCombatant>().OnHit(direction);
        }
        else if(GetComponentInParent<PlayerCombatant>() != null)
        {
            GetComponentInParent<PlayerCombatant>().OnHit();
        }
        //Trigger "OnHit" event for all instances of CombatCutscene
        var cutscenes = FindObjectsByType<CombatCutscene>(FindObjectsSortMode.None);
        foreach(var cutscene in cutscenes)        {
            cutscene.OnHit();
        }
    }

    public virtual void SlowMo(string message)
    {
        if(GetComponentInParent<EnemyCombatant>() != null)
        {
            var effect = Instantiate(Resources.Load<GameObject>("Particles/HitLight"), transform);
            //Time.timeScale = 0.125f;
        }
    }
}
