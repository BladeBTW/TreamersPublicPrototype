using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController _cc;//Character controller reference variable
    public float MoveSpeed = 5f;//Set to public so we can change it in the inspector in Unity
    private Vector3 _movementVelocity;
    private PlayerInput _playerInput;//Hold reference to player input script
    private float _verticalVelocity;//Used for vertical movement
    public float Gravity = -9.8f;//Set to public so we can change it in the inspector in Unity, gravity is negative since it's going down, same as eart
    private Animator _animator;//Holds reference to the animator controller

    public int Coin;

    // //Enemy
    public bool IsPlayer = true;//defines if this gameobject is a player or an enemy, set by true on default
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private Transform TargetPlayer;

    //Health
    private Health _health;

    //Damage caster
    private DamageCaster _damageCaster;

    // //Player slides
    private float attackStartTime;
    public float AttackSlideDuration = 0.4f;//Defines duration of the slide
    public float AttackSlideSpeed = 0.06f;//Defines the speed of the slide

    private Vector3 impactOnCharacter;

    public bool IsInvincible;
    public float invincibleDuration = 2f;//Defines duration of invincibility

    private float attackAnimationDuration;
    public float SlideSpeed = 9f;//Defines the speed of the slide

    // //State Machine
    public enum CharacterState
    {
        Normal, Attacking, Dead, BeingHit, Slide, Spawn
    }
    public CharacterState CurrentState;//Holds the current state of the character

    public float SpawnDuration = 1f;
    private float currentSpawnTime;

    //Material animation
    private MaterialPropertyBlock _materialPropertyBlock;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    public GameObject ItemToDrop;


    private void Awake()//This gets called when an instance of this script gets loaded, called before Start()
    {
        _cc = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _damageCaster = GetComponentInChildren<DamageCaster>();

        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();//Attached to the mesh game object
        _materialPropertyBlock = new MaterialPropertyBlock();//Initializes the material property block
        _skinnedMeshRenderer.GetPropertyBlock(_materialPropertyBlock);//Gets the material property block from the skinned mesh renderer

        if (!IsPlayer)
        {
            _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();//Gets the navmesh agent component from the gameobject this script is attached to
            TargetPlayer = GameObject.FindWithTag("Player").transform;//Finds the player gameobject and stores it in the TargetPlayer variable
            _navMeshAgent.speed = MoveSpeed;
            SwitchStateTo(CharacterState.Spawn);
        }
        else
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }


    private void CalculatePlayerMovement()
    {

        if (_playerInput.MouseButtonDown && _cc.isGrounded)//If the left mouse button is pressed and the character is grounded
        {
            SwitchStateTo(CharacterState.Attacking);
            return;
        }
        else if (_playerInput.SpaceKeyDown && _cc.isGrounded)
        {
            SwitchStateTo(CharacterState.Slide);
            return;
        }

        _movementVelocity.Set(_playerInput.HorizontalInput,0f,_playerInput.VerticalInput);//Sets the movement velocity from the player input script, value of y = 0 since no vert movement
        _movementVelocity.Normalize();//If we don't do this the player will move faster if moving diagonally
        _movementVelocity = Quaternion.Euler(0, -45f, 0) * _movementVelocity;//Changes the rotation of the movement velocity, to align with camera

        _animator.SetFloat("Speed", _movementVelocity.magnitude);//Sets the speed parameter in the animator controller to the magnitude of the movement velocity, which is the speed of the character

        _movementVelocity *= MoveSpeed * Time.deltaTime;//Multiply by Time.deltaTime to make it framerate independent, and makes the movement smooth across frames, Time.delatTime is the time it took to complete the last frame, so if the framerate is low, the movement will be slower, and vice versa
        if (_movementVelocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(_movementVelocity);//Rotates the character to face the direction of movement, using the movement velocity, which is already rotated to face the camera and a variable that can hold rotation values


        _animator.SetBool("AirBorne", !_cc.isGrounded);

        // RotateToCursor();
    }

    private void CalculateEnemyMovement()//If the distance between the enemeny and the player is bigger than the stopping distance, move towards the player, else stop moving and attack
    {
        if (Vector3.Distance(TargetPlayer.position, transform.position) >= _navMeshAgent.stoppingDistance)//By passing the player's position we are finding the distance between the enemy and the player, compare it to the stoppingDistance parameter in the naveMeshAgent component
        {
            _navMeshAgent.SetDestination(TargetPlayer.position);//If the distance is bigger than the stopping distance, move towards the player
            _animator.SetFloat("Speed", 0.2f);//Set the speed parameter in the animator controller to 0.2f (triggers the Run state in the animator controller)
        }
        else
        {
            _navMeshAgent.SetDestination(transform.position);//stay where it is
            _animator.SetFloat("Speed", 0f);//set speed to 0, to avoid running animation playing trigged in animation controller

            SwitchStateTo(CharacterState.Attacking);
        }
    }

    private void FixedUpdate()//Code that moves objects should always be put in FixedUpdate() function because it is called at a consistent rate.
    {
        switch (CurrentState)
        {
            case CharacterState.Normal:
                if (IsPlayer)
                    CalculatePlayerMovement();//Moves the character every (FixedUpdate()frame)
                else
                    CalculateEnemyMovement();//Move the enemey every (FixedUpdate()frame)
                break;

            case CharacterState.Attacking:

                if (IsPlayer)
                {  
                    if (Time.time < attackStartTime + AttackSlideDuration)
                    {
                        float timePassed = Time.time - attackStartTime;
                        float lerpTime = timePassed / AttackSlideDuration;
                        _movementVelocity = Vector3.Lerp(transform.forward * AttackSlideSpeed, Vector3.zero, lerpTime);
                    }

                    if(_playerInput.MouseButtonDown && _cc.isGrounded)
                    {
                        string currentClipName = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                        attackAnimationDuration = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                        if(currentClipName != "Attack03_Treamer" && attackAnimationDuration > 0.5f && attackAnimationDuration < 0.7f)
                        {
                            _playerInput.MouseButtonDown = false;
                            SwitchStateTo(CharacterState.Attacking);

                            //CalculatePlayerMovement();
                        }
                    }
                }
                
                break;

            case CharacterState.Dead:
                return;//Do nothing when character is dead

            case CharacterState.BeingHit:
                break;//Do nothing when character is being hit
            case CharacterState.Slide:
                _movementVelocity = transform.forward * SlideSpeed * Time.deltaTime;
                break;
            
            case CharacterState.Spawn:
                currentSpawnTime -= Time.deltaTime;
                if(currentSpawnTime <= 0)
                {
                    SwitchStateTo(CharacterState.Normal);
                }
                break;
        }

        
        if (impactOnCharacter.magnitude > 0.2f)
            {
                _movementVelocity = impactOnCharacter * Time.deltaTime;
            }
        impactOnCharacter = Vector3.Lerp(impactOnCharacter, Vector3.zero, Time.deltaTime * 5);
        
        if(IsPlayer)
        {
            if(_cc.isGrounded == false)//If the character is not grounded, apply gravity
                _verticalVelocity = Gravity;
            else
                _verticalVelocity = Gravity * 0.3f;//If the character is grounded, apply less gravity, this is to avoid the character controller thinking the character isNotGrounded when moving forwards in Unity (think of small slopes etc).
            
            _movementVelocity += _verticalVelocity * Vector3.up * Time.deltaTime;//Adds the vertical velocity to the movement velocity and time.deltaTime to make it framerate independent

            _cc.Move(_movementVelocity);//Moves the character every (FixedUpdate()frame)
            _movementVelocity = Vector3.zero;
        }
        else
        {
            if (CurrentState != CharacterState.Normal)
            {
                _cc.Move(_movementVelocity);
                _movementVelocity = Vector3.zero;
            }
        }

        
    }

    public void SwitchStateTo(CharacterState newState)
    {
        if (IsPlayer)
        {
            //Clear Cache
            _playerInput.ClearCache();
        }




        //Exiting state
        switch (CurrentState)
        {
            case CharacterState.Normal:
                break;

            case CharacterState.Attacking:

                if (_damageCaster != null)
                    DisableDamageCaster();

                if (IsPlayer)
                    GetComponent<PlayerVFXManager>().StopBlade();

                break;

            case CharacterState.Dead:
                return;//By making it return you make sure no other states are switched to after death

            case CharacterState.BeingHit:
                break;

            case CharacterState.Slide:
                break;

            case CharacterState.Spawn:
                IsInvincible = false;
                break;
        }

        //Entering state
        switch (newState)
        {
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:

                if (!IsPlayer)
                {
                    Quaternion newRotation = Quaternion.LookRotation(TargetPlayer.position - transform.position);
                    transform.rotation = newRotation;
                }

                _animator.SetTrigger("Attack");

                if (IsPlayer)
                {
                    attackStartTime = Time.time;
                    RotateToCursor();
                }

                break;
            case CharacterState.Dead:
                _cc.enabled = false;
                _animator.SetTrigger("Dead");
                StartCoroutine(MaterialDissolve());
                break;
            case CharacterState.BeingHit:
                _animator.SetTrigger("BeingHit");

                if(IsPlayer)//Only makes invincible if the character is the player
                {
                    IsInvincible = true;
                    StartCoroutine(DelayCancelInvincible());//Implements the delay as well as the cancel of the invincibility
                }

                break;
            case CharacterState.Slide:
                _animator.SetTrigger("Slide");
                GetComponent<SFXManager>().SFXPickupSmall();
                break;
            
            case CharacterState.Spawn:
                IsInvincible = true;
                currentSpawnTime = SpawnDuration;
                StartCoroutine(MaterialAppear());              
                break;
        }

        CurrentState = newState;

        Debug.Log("Switched to " + CurrentState);

    }

    public void SlideAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void DeathAnimationHitGround()
    {
        GetComponent<SFXManager>().SFXHitGround();
    }
    public void DeathAnimationStart()
    {
        GetComponent<SFXManager>().SFXDeathBell();
    }

    public void DeathAnimationStartNPC()
    {
        GetComponent<SFXManager>().SFXMonsterDeath();
    }

        public void DeathAnimationStartNPC2()
    {
        GetComponent<SFXManager>().SFXDeathMob();
    }

    public void attack1AnimationStarts()
    {
        GetComponent<SFXManager>().SFXSword_Swipe_5();
    }

    public void attack2AnimationStarts()
    {
        GetComponent<SFXManager>().SFXSword_Swipe_1();
    }

    public void attack3AnimationStarts()
    {
        GetComponent<SFXManager>().SFXSword_Swipe_2();
    }

    public void MeleeStatueAnimationHitGround()
    {
        GetComponent<SFXManager>().SFXStatue_GroundHit();
    }

    public void StatueFootAnimationHitFloor()
    {
        GetComponent<SFXManager>().SFXStatue_Walk();
    }





    public void AttackAnimationEnds()//Configured within animation clips, send on last frame
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void BeingHitAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void ApplyDamage(int damage, Vector3 attackerPos = new Vector3())//this function to keep it easy to manage from the character script as a hub
    {
        if (IsInvincible)
        {
            return;
        }

        if (_health != null)
        {
            _health.ApplyDamage(damage);
        }

        if (!IsPlayer)
        {
            GetComponent<EnemyVFXManager>().PlayBeingHitVFX(attackerPos);
        }

        StartCoroutine(MaterialBlink());//Coroutine material needs to get started

        if (IsPlayer)
        {
            SwitchStateTo(CharacterState.BeingHit);
            AddImpact(attackerPos, 10f);            
        }
        else
        {
            AddImpact(attackerPos, 2.5f);
        }
    }

    IEnumerator DelayCancelInvincible()
    {
        yield return new WaitForSeconds(invincibleDuration);
        IsInvincible = false;
    }

    private void AddImpact(Vector3 attackerPos, float force)
    {
        Vector3 impactDir = transform.position - attackerPos;
        Debug.Log("Attacked from position " + attackerPos);
        Debug.Log("Damage taken from position " + transform.position);
        Debug.Log("Vector 3 ImpactDir = " + impactDir);
        impactDir.Normalize();
        Debug.Log("Vector 3 ImpactDir.Normalize = " + impactDir);
        impactDir.y = 0;
        Debug.Log("Vector 3 ImpactDir.y set to 0 = " + impactDir);
        impactOnCharacter += impactDir * force;
    }

    public void EnableDamageCaster()
    {
        _damageCaster.EnableDamageCaster();
    }

    public void DisableDamageCaster()
    {
        _damageCaster.DisableDamageCaster();
    }

    IEnumerator MaterialBlink()
    {
                if (!IsPlayer)
        {
            GetComponent<SFXManager>().SFXHitStone();
        }
         
                if (IsPlayer)
        {
            GetComponent<SFXManager>().SFXPlayer_Damaged2();
        }
        _materialPropertyBlock.SetFloat("_blink", 0.4f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);//This is to avoid the material being reset to default when the animation ends

        yield return new WaitForSeconds(0.2f);//This is the duration of the blink, tells Unity to wait

        _materialPropertyBlock.SetFloat("_blink", 0f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);//This is to avoid the material being reset to default when the animation ends
    }

    IEnumerator MaterialDissolve()
    {
        yield return new WaitForSeconds(0.5f);

        float dissolveTimeDuration = 1.5f;
        float currentDissolveTime = 0;
        float dissolveHeight_start = 20f;
        float dissolveHeight_target = -30f;
        float dissolveHeight;

        _materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);//Update the change we just made

        while (currentDissolveTime < dissolveTimeDuration)//Animates height property over time
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHeight_start, dissolveHeight_target, currentDissolveTime / dissolveTimeDuration);
            _materialPropertyBlock.SetFloat("_dissolve_height", dissolveHeight);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;
        }

        DropItem();

    }

    public void DropItem()
    {
        if (ItemToDrop != null)
        {
            Instantiate(ItemToDrop, transform.position, Quaternion.identity);//.identity means no rotation
        }
    }

    public void PickUpItem(PickUp item)
    {
        switch (item.Type)
        {
            case PickUp.PickUpType.Heal:
                GetComponent<SFXManager>().SFXCrystalCollect();
                AddHealth(item.Value);
                break;
            case PickUp.PickUpType.Coin:
                AddCoin(item.Value);
                AddHealth(item.Value / 2);
                GetComponent<SFXManager>().SFXHealthCollect();
                break;
        }
    }

    private void AddHealth(int health)
    {
        _health.AddHealth(health);
        GetComponent<PlayerVFXManager>().PlayHealVFX();
        
    }

    private void AddCoin(int coin)
    {
        Coin += coin;
    }

    public void RotateToTarget()
    {
        if (CurrentState != CharacterState.Dead)//Checks if the character is dead
        {
            transform.LookAt(TargetPlayer, Vector3.up);
        }
    }

    IEnumerator MaterialAppear()
    {
        float dissolveTimeDuration = SpawnDuration;
        float currentDissolveTime = 0;
        float dissolveHight_start = -10f;
        float dissolveHight_target = 20f;
        float dissolveHight;

        _materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);

        while (currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHight = Mathf.Lerp(dissolveHight_start, dissolveHight_target, currentDissolveTime / dissolveTimeDuration);
            _materialPropertyBlock.SetFloat("_dissolve_height", dissolveHight);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;
        }

        _materialPropertyBlock.SetFloat("_enableDissolve", 0f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    private void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitResult;

        if (Physics.Raycast(ray, out hitResult, 1000, 1 << LayerMask.NameToLayer("CursorTest")))
        {
            Vector3 cursorPos = hitResult.point;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cursorPos, 1);
        }
    }

    private void RotateToCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitResult;

        if (Physics.Raycast(ray, out hitResult, 1000, 1 << LayerMask.NameToLayer("CursorTest")))
        {
            Vector3 cursorPos = hitResult.point;
            transform.rotation = Quaternion.LookRotation(cursorPos - transform.position, Vector3.up);
        }
    }
}
