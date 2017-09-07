using UnityEngine;
using System.Collections;

public class AdvancedController : MonoBehaviour {

	public bool infinityRunner = false;
	public bool allowCrouch = true;
	public bool allowJump = true;
	public bool allowAutomaticJump = false;
	public bool allowFloat = false;
	public bool allowWallJjump = true;
	public bool allowFight = true;

	public string HorizontalMoveInput;
	public string VerticalMoveInput;
	public string CrouchInput;
	public string JumpInput;
	public string FightInput;

	public string jumpUpTag;
	public string climbTag;
	public string jumpObstacleTag;
	public string jumpWallTag;
	public LayerMask groundLayer;


    public float speedDampTime = 0.1f;	// The damping for the speed parameter
    public Transform rightHand;
    public float velocity = 10;
    public float climbVelocity = 1;
	public int totalOfJumps = 1;
	private int jumpsRestantes;
	private Animator animator;				// Reference to the animator component.
    //private DoneHashIDs hash;			// Reference to the HashIDs.
    private float horizontal;
    private float vertical;
    private bool sneak;

    private bool climbing;
    private bool isJumpingUp;
    private Rigidbody rigidBody;
    private Vector3 move;
    private float lastMoveX;
    private float verticalVelocity;
    public float jumpForce = 500000.0f;

    private float lastTimePunchPressed = 0.0f;
    public float timeForCombo = 0.5f;
    private float startTime = 0.0f;
    private float timeWithoutFight = 0.0f;
    private float startTimeWithoutFight = 0.0f;

    private bool isGrounded = false;

	public float climbDistance = 0.5f;

	private Vector3 topCollider;
	public bool swimming = false;

	private float rotationX;
	private float waterLimitY;

	private AnimatorStateInfo stateInfo;
	private float collideWithWall = 0;

	private CapsuleCollider footCollider;
	public float radius = 1;
	public float height = 1;
	public Vector3 capsuleColliderCenter = Vector3.zero;

	private float distanceCorrection = 0;
	public float offsetHandClimb = 0;

	//Mis variables
	bool staticAnimation = false;
	Rigidbody playerBody;
	float timer = 0f;
	float jumpUpTimer = 1.15f;
	bool isJumpCoroutine = false;
	float runJumpTime = 2.5f;
	float runJumpTimer = 0f;
	bool isJumpRunCoroutine = false;
	float jumpDistance = 4f;
	float jumpHeight = 1f;
	float jumpRunSpeedFactor = 3f;
	Vector3 startPos;
	Vector3 destPos;
	Vector3 frameVelocity = new Vector3(0f,-1f);
	Vector3 frameAcceleration = new Vector3(0f,-1f);
	//Vector3 oldVelocity = new Vector3 (0f, 0f);
	float velocityY = 0f;
	//float accelerationY = 0f;
	//float oldAcceleration = 0f;
	//bool enterAcceleration = true;
	float fallSmoothFactor = 0.4f;
	int fallingCounter = 1;
	Vector3 lastPosition;
	float initialZPos;
	// Jump Up variables
	public float jumpSmoothFactor = 1f;
	public float jumpTimer = 1.9f;
	public float groundCorrection = 0.1f;

	//Velocity Y correction variables
	float oldVelocityY = -1f;
	ArrayList velocityArray = new ArrayList();
	ArrayList isGroundedArray = new ArrayList();
	string arrayToString = "";

	float groundDetectionSensitivity = 0.1f;
	public int horizontalLineCount = 4; //amount of horizontal front rays to detect objects in front of the character
	public int verticalLineCount = 4; //amount of vertical rays to detect ground or objects below the character
	float horizontalLineSpacing; //space between each horizontal ray
	float verticalLineSpacing; 
	Vector3 playerWidth;
	Vector3 playerHeight;
	Vector3 rightDistance; //distance from the center of the character to the right
	Vector3 leftDistance; 

    void Start() {
        startTime = Time.time;
		jumpsRestantes = totalOfJumps;
		playerBody = this.rigidBody;
    }

    void Awake()
    {
        // Setting up the references.

		PhysicMaterial noFrictionPhysycMaterial = new PhysicMaterial();
		noFrictionPhysycMaterial.dynamicFriction = 0.0001f;
		noFrictionPhysycMaterial.staticFriction = 0.09f;
		noFrictionPhysycMaterial.bounciness = 0;
		footCollider = this.gameObject.AddComponent<CapsuleCollider>();
		footCollider.material = noFrictionPhysycMaterial;
		footCollider.center = capsuleColliderCenter;
		footCollider.height = height;
		footCollider.radius = radius;
		this.rigidBody =  this.gameObject.AddComponent<Rigidbody>();
		this.rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionZ;

		// Mi codigo
		this.rigidBody.isKinematic = true; 
		this.rigidBody.useGravity = false;
		this.rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		this.rigidBody.interpolation = RigidbodyInterpolation.Interpolate;

		lastPosition = transform.position;
		initialZPos = transform.position.z;
		// Hasta aqui
	
        animator = GetComponent<Animator>();

    }

    void FixedUpdate()
    {
		Float();

		// Velocity calculations
		oldVelocityY = velocityY;
		velocityY = Mathf.Round(currentVelocity (lastPosition)*10f);

		velocityArray.Add (velocityY);
		isGroundedArray.Add (isGrounded);

		// Jump timer
		if (timer > 0) {
			jumpSmoothFactor = 0.9f;
			groundDetectionSensitivity = 0.07f;
			timer -= Time.deltaTime;
			animator.SetFloat ("VerticalSpeed", velocityY);
			if ((stateInfo.IsName ("Base Layer.Jump") && !isGrounded || isJumpCoroutine)) {
				Debug.Log ("Entro corutina");
				StartCoroutine ("JumpCoroutine");
			}
			if ((stateInfo.IsName ("Base Layer.JumpRunning") && !isGrounded) && !isJumpRunCoroutine) {
				StartCoroutine (JumpRunCoroutine (destPos, runJumpTime));
			}
		} else if (!isGrounded && (stateInfo.IsName ("Base Layer.Jump") || stateInfo.IsName ("Base Layer.JumpRunning"))) {
			isJumpCoroutine = false;
			timer = 0f;
			StopCoroutine ("JumpCoroutine");
			StopCoroutine ("JumpRunCoroutine");
			if (!staticAnimation) {
				if (velocityY < 80f && animator.GetFloat ("Speed") < 5) {
					Debug.Log ("Paro corutina");
					animator.SetFloat ("VerticalSpeed", -1f);
				}
			}
		} /*else if (stateInfo.IsName ("Base Layer.JumpObstacle")) {
			groundDetectionSensitivity = 0.07f;
		}*/
		else {
			groundDetectionSensitivity = 0.7f;
			Debug.Log ("En medio");

			if (!isGrounded && !animator.GetBool("climbing") && !stateInfo.IsName("Base Layer.OverWall"))
				velocityY = -1f;
			animator.SetFloat ("VerticalSpeed", velocityY);
		}

		if ((staticAnimation && stateInfo.IsName ("Base Layer.JumpRunning"))) {
			velocityY = 1f;
			animator.SetFloat ("VerticalSpeed", velocityY);
		}
			
		animator.SetFloat ("Timer",timer);

		float groundColliderY = 0;
		groundColliderY = checkGroundCollision();
		positionCorrection (groundColliderY);

		if (isGrounded)
			velocityY = 0;

		lastPosition = transform.position;

		CalculateRaySpacing ();
		Jump();	
    }


    void Update()
    {
		//Debug.Log (isGrounded);
		if (Input.GetKey (KeyCode.G)) {
			for (int i = 0; i < velocityArray.Count; i++) {
				arrayToString += velocityArray [i] + " b " + isGroundedArray [i] + " b ";
			}
			//Debug.Log ("Count: "+ velocityArray.Count);
			Debug.Log (arrayToString);
		}
		stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        horizontal = infinityRunner ? 1 : Input.GetAxis(HorizontalMoveInput);
        vertical = Input.GetAxis(VerticalMoveInput);
		//Debug.Log ("Speed: " + animator.GetFloat("Speed"));
		MovementManagement(horizontal, vertical);
		jumpAndClimb();
		JumpObstacle();
		Slide();
        
		JumpRunning();
        Climb();
        crouch();
        fight();
		WallJump();
		fall();
		footCollider.isTrigger = animator.GetFloat("Collider") == 0 ? true : false;

		// Salto en el lugar, el 20f determina la altura

    }

	void CalculateRaySpacing(){ 

		Bounds bounds = footCollider.bounds; //bounds of the collider
		//Vector3 colliderOffset = footCollider.; //offset of the collider

		// Correct the bounds of the collider with the offset
		float boundsMaxY = bounds.max.y /*- colliderOffset.y*/;
		float boundsMinY = bounds.min.y /*+ colliderOffset.y*/;
		float boundsMaxX = bounds.max.x /*- colliderOffset.x*/;
		float boundsMinX = bounds.min.x /*+ colliderOffset.x*/;

		// Define the number of lines, their space and clamp its values
		horizontalLineCount = Mathf.Clamp (horizontalLineCount,2,int.MaxValue);
		verticalLineCount = Mathf.Clamp (verticalLineCount,2,int.MaxValue);

		horizontalLineSpacing = (bounds.size.y)/(horizontalLineCount - 1);
		verticalLineSpacing = (bounds.size.x) /(verticalLineCount - 1);

		// Size of the vertical and horizontal lines
		playerHeight = new Vector3 (0f, boundsMaxY - boundsMinY);
		playerWidth = new Vector3 (boundsMaxX - boundsMinX,0f);
		rightDistance = new Vector3 (-(boundsMaxX - boundsMinX)/2, 0f);
		leftDistance = new Vector3 (-(boundsMaxX - boundsMinX)/2, 0f);
		//Debug.Log ("playerHeight " + playerHeight);
		//Debug.Log ("playerWidth " + playerWidth);
		//Debug.Log ("rightDistance " + rightDistance);
		//Debug.Log ("leftDistance " + leftDistance);
	}

	void positionCorrection(float groundColliderY){
		// Mi codigo
		// Por alguna razón después de un par de saltos se va corriendo en el eje z, esto lo corrige

		if (transform.position.z != initialZPos){
			this.transform.position = new Vector3 (this.transform.position.x, this.transform.position.y, initialZPos);
		}

		// Corrección de suelo, para evitar q se hunda, con una variable de ajuste publica que se llama groundCorrection
		if (isGrounded && !stateInfo.IsName("Base Layer.Jump") && !stateInfo.IsName("Base Layer.Climb")/*&& !animator.IsInTransition(0) && (stateInfo.IsName("Base Layer.Locomotion.Run") || stateInfo.IsName("Base Layer.Locomotion.Crouch") 
			|| stateInfo.IsName("Base Layer.Locomotion.Land") || stateInfo.IsName("Base Layer.Locomotion.Idle") || stateInfo.IsName("Base Layer.Locomotion.Falling"))*/) {
			this.transform.position = new Vector3 (this.transform.position.x, groundColliderY + groundCorrection, this.transform.position.z);
		}

	}

	IEnumerator JumpCoroutine(){
		while(timer > 0 && !isGrounded) {
			if(!staticAnimation)
				this.transform.Translate (Vector3.up * Time.deltaTime * jumpSmoothFactor);
			yield return null;
		}
	}

	IEnumerator JumpRunCoroutine(Vector3 destination, float time){
		runJumpTimer = 0f;
		isJumpRunCoroutine = true;
		//this.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
		while(runJumpTimer <= 1.0f) {
			Debug.Log (runJumpTimer);
			animator.SetFloat ("Speed", 6f);
			runJumpTimer += Time.deltaTime / time * jumpRunSpeedFactor;
			float height = Mathf.Sin (Mathf.PI * runJumpTimer) * jumpHeight;
			//Debug.Log (runJumpTimer + "" + isGrounded);
			if (!staticAnimation && runJumpTimer > 0.1f) {
				this.transform.position = Vector3.Lerp (startPos, destination, runJumpTimer) + Vector3.up * height;
			}
			//this.transform.Translate (new Vector3(0f, /*smoothFactor / 5f * Time.deltaTime)*/ 0f , horizontal * smoothFactor * Time.deltaTime * horizontal));
			yield return null;
		}
		isJumpRunCoroutine = false;
	}

	void JumpRunning() {
		if(!allowJump) return;
		if(stateInfo.IsName("Base Layer.JumpRunning") && jumpsRestantes > 0){
			if (Input.GetButtonDown(JumpInput))
			{
				animator.SetBool("AirJump", true);
				jumpsRestantes -= 1;
			}
			else {
				animator.SetBool("AirJump", false);
			}

		}
		if (stateInfo.IsName("Base Layer.Locomotion.Run"))
		{
			if (Input.GetButtonDown(JumpInput))
			{
				animator.SetBool("JumpRunning", true);
				jumpsRestantes -= 1;
				//isJumpRunCoroutine = true;
				startPos = transform.position;
				if(getCurrentSide() == 1)
					destPos = new Vector3 (transform.position.x + jumpDistance, transform.position.y, transform.position.z);
				else
					destPos = new Vector3 (transform.position.x - jumpDistance, transform.position.y, transform.position.z);
				timer = runJumpTime;
			}
			else {
				animator.SetBool("JumpRunning", false);
			}
		}
		if(stateInfo.IsName("Base Layer.Locomotion.Run") || stateInfo.IsName("Base Layer.Locomotion.Idle") && !animator.IsInTransition(0))
			jumpsRestantes = totalOfJumps;

	}

	void Jump()
	{
		if(!allowJump) return;
		if (stateInfo.IsName("Base Layer.Locomotion.Idle") && !animator.GetBool("JumpUp"))
		{
			if (Input.GetButtonDown(JumpInput))
			{
				animator.SetBool("Jump", true);
				//Mi codigo
				timer = jumpUpTimer;
				isJumpCoroutine = true; 
				//
			}
		}
		else
		{
			animator.SetBool("Jump", false);
		}
	}

	void JumpObstacle(){
		if(!allowAutomaticJump) return;
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		RaycastHit hit;
		Debug.DrawLine (transform.position, fwd, Color.black);
		if (!stateInfo.IsName("JumpObstacle") && Physics.Raycast(transform.position, fwd, out hit, 2.0f) && hit.transform.tag == jumpObstacleTag)
		{
			isGrounded = true;
			getTopCollider(hit);
			topCollider = new Vector3(topCollider.x + (((BoxCollider)hit.collider).size.x / 2) * getCurrentSide(),topCollider.y,topCollider.z);
			if (stateInfo.IsName("Base Layer.Locomotion.Run") && !animator.IsInTransition(0))
			{
				animator.SetBool("JumpObstacle", true);
			}
		}
		else{
			animator.SetBool("JumpObstacle", false);
		}
		if (stateInfo.IsName("Base Layer.JumpObstacle") && !animator.IsInTransition(0))
		{
			isGrounded = true;
			animator.MatchTarget(topCollider, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(new Vector3(1, 1, 1), 0), 0.14f, 0.536f);
		}
	}

	void Climb() {
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		RaycastHit hit;
		float totalClimbDistance = climbDistance;
		if(animator.GetBool((Animator.StringToHash("climbing"))) &&  !stateInfo.IsName("Base Layer.OverWall")){
			totalClimbDistance += 1.0f;
			animator.SetFloat(Animator.StringToHash("climbSpeed"), vertical * 5.0f, 0.2f, Time.deltaTime);
			if(isGrounded && vertical < 0){
				climbing = false;
				animator.SetBool("climbing", false); 
				distanceCorrection = 0;
			}
				Vector3 correctedPosition = transform.position;
				correctedPosition.x = distanceCorrection + (offsetHandClimb * getCurrentSide ());
				transform.position = correctedPosition;
		}
		Debug.DrawRay (transform.position + (Vector3.up * 1.0f), fwd, Color.blue);
		if (Physics.Raycast(transform.position + (Vector3.up * 1.0f), fwd, out hit, totalClimbDistance) && hit.transform.tag == climbTag)
		{
			getTopCollider(hit);
			if (stateInfo.IsName("Base Layer.Locomotion.Idle"))
			{
				if (vertical > 0) {
					climbing = true;
					animator.SetBool("climbing", true); 

					rigidBody.MovePosition(transform.position + transform.up * climbVelocity * Time.deltaTime * vertical);
				}
			}
			Debug.DrawLine(transform.position, topCollider, Color.red);
		}

		if (animator.GetBool((Animator.StringToHash("climbing"))) && Vector3.Distance(topCollider, transform.position) < 1.0f) {
			animator.SetFloat(Animator.StringToHash("climbSpeed"), 0);
			animator.SetBool("OverWall", true);
			animator.SetBool("climbing", false); 
			climbing = false;
		}
		if ( stateInfo.IsName("Base Layer.OverWall") && !animator.IsInTransition(0) )
		{
			distanceCorrection = 0;
			animator.SetBool("OverWall", false);
		}

	}

	float checkGroundCollision(){
		int layer = groundLayer;
		Vector3 linePosition = new Vector3 (footCollider.bounds.center.x, footCollider.bounds.min.y, footCollider.bounds.center.z);
		float groundColliderMaxY = 0;

		RaycastHit verticalHit = new UnityEngine.RaycastHit(); // Saves the last vertical hit

		if (getCurrentSide() == 1) { // Detection of whats below the player
			for (int e = 1; e < verticalLineCount; e++) {
				Debug.DrawLine (linePosition + playerHeight / 2f + playerWidth / 2 + Vector3.left * verticalLineSpacing * e, linePosition + groundDetectionSensitivity * Vector3.down + playerWidth / 2 + Vector3.left * verticalLineSpacing * e, Color.black);
				if (Physics.Linecast (linePosition + playerHeight / 2 + playerWidth / 2// The vertical LineCast has to start in front of the player
				    + Vector3.left * verticalLineSpacing * e // The lines go from left to right (player looking to the left)
					, linePosition + groundDetectionSensitivity * Vector3.down + playerWidth / 2 + Vector3.left * verticalLineSpacing * e // The LineCast end point has to be the bottom of the player collider
					, out verticalHit
					,1 << layer)) {
					isGrounded = true;
					groundColliderMaxY = verticalHit.collider.bounds.max.y;
					e = verticalLineCount;
				} else {
					isGrounded = false;
				}
			}
		} else {
			for(int e = 1; e < verticalLineCount; e++){	
				Debug.DrawLine (linePosition + playerHeight / 2 - playerWidth / 2 + Vector3.right * verticalLineSpacing * e, linePosition + groundDetectionSensitivity * Vector3.down - playerWidth / 2 + Vector3.right * verticalLineSpacing * e, Color.black);
				if (Physics.Linecast (linePosition + playerHeight / 2 - playerWidth / 2 + Vector3.right * verticalLineSpacing * e, linePosition + groundDetectionSensitivity * Vector3.down - playerWidth / 2 + Vector3.right * verticalLineSpacing, out verticalHit, 1 << layer)) {
					isGrounded = true;
				 	groundColliderMaxY = verticalHit.collider.bounds.max.y;
					e = verticalLineCount;
//					return groundColliderMaxY;
				} 
				else {
					isGrounded = false;
				}
			}
		}
		if (isGrounded) {
			return groundColliderMaxY;
		}
		else
			return 22;

		/*for (int e = 0; e < 3 - 1; e++) {
			Debug.DrawLine (footCollider.bounds.center, new Vector3 (footCollider.bounds.center.x, footCollider.bounds.min.y, footCollider.bounds.center.z), Color.blue);
			if (Physics.Linecast (footCollider.bounds.center, new Vector3 (footCollider.bounds.center.x, footCollider.bounds.min.y, footCollider.bounds.center.z), out verticalHit, 1 << layer)) {
				isGrounded = true;
				float groundColliderMaxY = verticalHit.collider.bounds.max.y;
				return groundColliderMaxY;
			} else {
				isGrounded = false;
				return 0;
			}
		}*/
		//Collider[] colliderArray;
		//float radiusCheck = footCollider.radius * 0.9f;
		//Vector3 posCheck = new Vector3(this.transform.position.x + capsuleColliderCenter.x, this.transform.position.y + capsuleColliderCenter.y, this.transform.position.z + capsuleColliderCenter.z) - Vector3.up * (radiusCheck*0.1f);
		//float radiusCheck = footCollider.radius * 0.9f;
		//Vector3 pos = footCollider.bounds.min;
		//Vector3 pos = transform.position + Vector3.up * (radiusCheck*0.80f);
		//int layerMask = 1 << this.groundLayer.value;
		//isGrounded = Physics.CheckSphere(pos,radius,layerMask);
		//colliderArray = Physics.OverlapSphere (pos,radius,layerMask);
		//isGrounded = Physics.CheckCapsule(posCheck - new Vector3(0,height / 2,0), posCheck + new Vector3(0,height / 2,0), radiusCheck, layerMask);

		//colliderArray = Physics.OverlapCapsule(posCheck - new Vector3(0,height / 2,0), posCheck + new Vector3(0,height / 2,0), radiusCheck, layerMask);
		//if (colliderArray.Length > 0) {
		//	isGrounded = true;
			//Debug.Log ("Ground Y: " + colliderArray [0].gameObject.transform.position.y);
		//	Collider groundCollider = colliderArray [0];
			//Debug.Log ("Ground Y: " + groundCollider.bounds.max.y);
			//Debug.Log ("Foot Correction: " + radiusCheck);
			//Debug.Log ("Ground Y: " + colliderArray [0].gameObject);
			//CapsuleCollider playerCollider = GetComponent<CapsuleCollider> ();
			//Debug.Log ("Player Collider MaxY: " + playerCollider.bounds.max.y);
			//Debug.Log ("Player Collider MinY: " + playerCollider.bounds.min.y);
		//	return groundCollider.bounds.max.y;
		//}
		//else
		//	return 0;

	}


	void OnDrawGizmosSelected() {
		float radius = this.radius;
		Vector3 pos = new Vector3(this.transform.position.x + capsuleColliderCenter.x, this.transform.position.y + capsuleColliderCenter.y, this.transform.position.z + capsuleColliderCenter.z);
		Gizmos.color = Color.yellow;
		//Gizmos.DrawWireSphere(pos, radius);
		//Gizmos.DrawWireSphere(pos + new Vector3(0,1,0), radius);
		DebugExtension.DrawCapsule(pos - new Vector3(0,height / 2,0), pos + new Vector3(0,height / 2,0), Color.blue, radius);
		if(footCollider != null){
			float radiusCheck = footCollider.radius * 0.95f;
			Vector3 posCheck = new Vector3(this.transform.position.x + capsuleColliderCenter.x, this.transform.position.y + capsuleColliderCenter.y, this.transform.position.z + capsuleColliderCenter.z) - Vector3.up * (radiusCheck*0.1f);
			Gizmos.color = Color.red;
			//Gizmos.DrawSphere(posCheck, radiusCheck);
			DebugExtension.DrawCapsule(posCheck - new Vector3(0,height / 2,0), posCheck + new Vector3(0,height / 2,0), Color.red, radiusCheck);
		}

	}

	void swim(){
		animator.SetBool("touchWater", false);

		if(swimming && !animator.GetBool("Swimming")){
			animator.SetBool("touchWater", true);
			animator.SetBool("Swimming", true);
			rigidBody.MovePosition(transform.position + new Vector3(0,-1.0f,0));
			waterLimitY = transform.position.y - 1.0f;
		}
		if(animator.GetBool("Swimming")){
			if(transform.position.y > waterLimitY)
				rigidBody.MovePosition(new Vector3(transform.position.x, waterLimitY, transform.position.z));
			animator.SetFloat(Animator.StringToHash("Speed"), Mathf.Abs(horizontal) * 1.0f, 1.0f, Time.deltaTime);
			if(horizontal != 0){
				//if(transform.eulerAngles.x <= 360 && transform.eulerAngles.x > 270)
				Debug.LogError(rotationX);
				rotationX += vertical * 1;
				rotationX = Mathf.Clamp(rotationX, -80.0f, 80.0f);
				rigidBody.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionZ;
				//transform.localEulerAngles = new Vector3(rotationX, transform.localEulerAngles.y, transform.localEulerAngles.z);
				if(rigidBody.angularVelocity.z < 0.5f){
					if(vertical > 0){
						rigidBody.angularVelocity = new Vector3(rigidBody.angularVelocity.x, rigidBody.angularVelocity.y, 0.9f);
						rotationX += Time.deltaTime * 1;
					}else if(vertical < 0){
						rigidBody.angularVelocity = new Vector3(rigidBody.angularVelocity.x, rigidBody.angularVelocity.y, -0.9f);
						rotationX += Time.deltaTime * -1;
					}
				}
					//rigidBody.AddTorque(new Vector3(0, 0,0.1f * v));
				if(vertical == 0.0f || (rotationX > 60 || rotationX < -80))
					rigidBody.angularVelocity = new Vector3(rigidBody.angularVelocity.x, rigidBody.angularVelocity.y, 0);
				if(horizontal == 0.0f){
					
				}
				//transform.rotation = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
				
				//if((transform.eulerAngles.x > 300 && v < 0) || (transform.eulerAngles.x < 80 && v > 1))
				//transform.Rotate(new Vector3(1,0.0f,0.0f),1.0f * v ,Space.Self);
			}else{
				if(transform.rotation.eulerAngles.x != 0){
					
					rigidBody.constraints =  RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ;
					transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.localEulerAngles.y, 0), Time.time * 0.005f);
					//transform.rotation = Quaternion.Euler(0, transform.localEulerAngles.y, transform.localEulerAngles.z);	
				}
				rigidBody.constraints =  RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ;

				rotationX = 0.0f;
			}
		}

	}

	void fall(){
		stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if(!isGrounded && 
			(velocityY < 0.2f)
			&& !climbing &&  !stateInfo.IsName ("Base Layer.JumpObstacle") /*&& !stateInfo.IsName ("Base Layer.Jump") !stateInfo.IsName ("Base Layer.JumpRunning") &&*/
			/*((stateInfo.IsName("Base Layer.Locomotion.Run") || stateInfo.IsName("Base Layer.Locomotion.Idle")) && !animator.IsInTransition(0))
			|| (stateInfo.IsName("Base Layer.JumpRunning") && stateInfo.normalizedTime >= 1.0f)*/){
			this.transform.Translate (new Vector3 (0f, fallingCounter / 10 * Physics.gravity.y * Time.smoothDeltaTime * fallSmoothFactor, /*horizontal * 10f * Time.deltaTime*/0f));
			fallingCounter++;
			animator.SetBool("Falling", true);
		}else{
			animator.SetBool("Falling",false);
			fallingCounter = 1;
		}
		if(isGrounded){
			animator.SetBool("grounded", true);
		}else{
			animator.SetBool("grounded", false);
		}
	}


    void MovementManagement(float horizontal, float vertical)
    {
		float velocityCollide = 0.0f;
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		RaycastHit hit;
		if (Physics.Raycast(transform.position + (Vector3.up * 1.0f), fwd, out hit, 0.5f))
		{
			collideWithWall =  Mathf.SmoothDamp(collideWithWall, getCurrentSide(), ref velocityCollide, 0.05f); 
			Debug.DrawLine(transform.position + (Vector3.up * 1.0f), transform.position + (Vector3.up * 1.0f) + (getCurrentSide() * Vector3.right * (footCollider.radius + 0.1f)), Color.red);	

		}else{
			collideWithWall = 0;
		}
        // Set the sneaking parameter to the sneak input.
        //anim.SetBool(hash.sneakingBool, sneaking);

        // If there is some axis input...
        if (horizontal != 0f )
        {
            Rotating(horizontal);
			if(swimming)
				return;
			if ((horizontal < 0 && collideWithWall < -0.9) || (horizontal > 0 && collideWithWall > 0.9)) {
				//float speed = Mathf.SmoothDamp(animator.GetFloat("Speed"),0.0f,ref velocity2, 0.3f);
				animator.SetFloat (Animator.StringToHash ("Speed"), 0);
				//Debug.LogError("velocidad " + animator.GetFloat("Speed"));
			} else{
				animator.SetFloat (Animator.StringToHash ("Speed"), Mathf.Abs (horizontal) * 5.5f, speedDampTime, Time.deltaTime);
			}

            //rigidBody.MovePosition(transform.position + transform.forward * velocity * Time.deltaTime);
        }
        else
        {
			if(swimming)
				return;
            // Otherwise set the speed parameter to 0.
            animator.SetFloat(Animator.StringToHash("Speed"), 0);
            
        }
    }


    void Rotating(float horizontal)
    {
		if (horizontal > 0) { 
			horizontal = 1; 
		} else if (horizontal < 0) { 
			horizontal = -1; 
		}else{ 
			return;
		}
        if ((stateInfo.IsName("Base Layer.Locomotion.Run") || stateInfo.IsName("Base Layer.Locomotion.Idle") 
			|| stateInfo.IsName("Base Layer.Crouch.Idle") || stateInfo.IsName("Base Layer.Crouch.Walk")
			|| stateInfo.IsName("Base Layer.Swim") || stateInfo.IsName("Base Layer.Float")) && !animator.GetBool("JumpObstacle") &&  !stateInfo.IsName("Base Layer.JumpObstacle") || stateInfo.IsName("Base Layer.Fight.Walk"))

		{
			if(rigidBody.angularVelocity.z == 0)
				transform.rotation = Quaternion.Euler(transform.localEulerAngles.x, 90 * horizontal, transform.localEulerAngles.z);
        }
    }

    void Slide() {
        if (stateInfo.IsName("Base Layer.Locomotion.Run"))
        {
            if (Input.GetButton("Fire1")) animator.SetBool("Slide", true);
        }
        else
        {
            animator.SetBool("Slide", false);
        }
    }

    

	void Float(){
		if(!this.allowFloat) return;
		if (stateInfo.IsName("Base Layer.Jump") && !animator.IsInTransition(0))
		{
			if (Input.GetButton("Jump"))
			{
				animator.SetBool("Float", true);
				rigidBody.useGravity = false;
			}
		}
		if (stateInfo.IsName("Base Layer.Float"))
		{
			float verticalForce = 0;
			if (Input.GetButton("Jump"))
			{
				rigidBody.useGravity = false;
				verticalForce = 2 * Time.deltaTime;
			}
			rigidBody.MovePosition(transform.position + (new Vector3(horizontal * Time.deltaTime * 10, verticalForce,0 )));
		}
		if(!Input.GetButton("Jump")){
			rigidBody.useGravity= true;
		}
		if(isGrounded)
			animator.SetBool("Float", false);
	}


		

    

    void jumpAndClimb() {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, fwd, out hit, 0.5f) && hit.transform.tag == jumpUpTag)
        {
			getTopCollider(hit);


			if (Input.GetButton(JumpInput) && stateInfo.IsName("Base Layer.Locomotion.Idle"))
            {
                animator.SetBool("JumpUp", true);
            }

			if (stateInfo.IsName("Base Layer.JumpAndHang") && !animator.IsInTransition(0))
            {
                animator.SetBool("JumpUp", false);
				animator.MatchTarget(topCollider + new Vector3(0.12f * getCurrentSide() * -1 ,0.12f,0), Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(new Vector3(1, 1, 1), 0), 0.432f, 0.57f);
            }


        }
		if ( stateInfo.IsName("Base Layer.OverWall") && !animator.IsInTransition(0))
		{
			animator.MatchTarget(topCollider + new Vector3(0.12f * getCurrentSide() * -1,0.12f,0), Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(new Vector3(1, 1, 1), 0), 0.0f, 0.01f);
			Debug.DrawLine(transform.position, topCollider);
		}
		if (stateInfo.IsName("Base Layer.JumpAndHang"))
		{
			if(Input.GetButtonDown(JumpInput))
				isJumpingUp = true;
		}
		if(isJumpingUp){
			if(stateInfo.IsName("Base Layer.JumpAndHang") && stateInfo.normalizedTime >= 1){
				animator.Play("Base Layer.OverWall");
				isJumpingUp = false;
			}
		}

    }

    void crouch() {
		if(!allowCrouch) return;
        if (stateInfo.IsName("Base Layer.Locomotion.Idle"))
        {
            if (vertical < 0) animator.SetBool("Crouch", true);
            if (Mathf.Abs(horizontal) > 0 && vertical < 0) animator.SetFloat(Animator.StringToHash("Speed"), 1);
        }
        else if ((stateInfo.IsName("Base Layer.Crouch.Idle") || stateInfo.IsName("Base Layer.Crouch.Walk")) && vertical >= 0)
        {
            animator.SetBool("Crouch", false);
        }
        if (stateInfo.IsName("Base Layer.Crouch.Walk") && Mathf.Abs(horizontal) == 0) {
            animator.SetFloat(Animator.StringToHash("Speed"), 0);
        }
    }

    void fight() {
		if (Input.GetButtonDown(FightInput)) {
            startTime = Time.time;
            //startTimeWithoutFight = 0.0f;
        }
        lastTimePunchPressed = Time.time - startTime;
        timeWithoutFight = Time.time - startTimeWithoutFight;
        if (stateInfo.IsName("Base Layer.Locomotion.Idle") || stateInfo.IsName("Base Layer.Locomotion.Run"))
        {
			if (Input.GetButtonDown(FightInput))
            {
                animator.SetBool("Punch", true);
                animator.SetBool("Fighting", true);
				startTimeWithoutFight = Time.time;
            }
            
        }
		if (stateInfo.IsName("Base Layer.Fight.Punch") || stateInfo.IsName("Base Layer.Fight.SecondPunch"))
        {
			if (Input.GetButtonDown(FightInput))
            {
                if (lastTimePunchPressed < timeForCombo)
                {
                    animator.SetBool("Punch", true);
					startTimeWithoutFight = Time.time;
                }
                else {
                    animator.SetBool("Punch", false);
                    startTimeWithoutFight = Time.time;
                }
            }
            
            if (lastTimePunchPressed > timeForCombo)
            {
                animator.SetBool("Punch", false);
                startTimeWithoutFight = Time.time;
            }
        }
        if (stateInfo.IsName("Base Layer.Fight.Idle") || stateInfo.IsName("Base Layer.Fight.Walk")) {
			if (Input.GetButtonDown(FightInput))
			{
					animator.SetBool("Punch", true);
					startTimeWithoutFight = Time.time;
			}
            if (timeWithoutFight > 8.0f) {
                animator.SetBool("Fighting", false);
            }
        }
    }

	void WallJump(){
		if(!allowWallJjump) return;
		if ((stateInfo.IsName("Base Layer.JumpRunning") || stateInfo.IsName("Base Layer.Jump") || stateInfo.IsName("Base Layer.AirJump")) && !animator.IsInTransition(0))
		{
			Vector3 fwd = transform.TransformDirection(Vector3.forward);
			RaycastHit hit;
			if (Input.GetButton(JumpInput) && Physics.Raycast(transform.position, fwd, out hit, 0.5f) && hit.transform.tag == jumpWallTag)
			{
				
				transform.rotation = Quaternion.Euler(transform.localEulerAngles.x,  getCurrentSide() * -90, transform.localEulerAngles.z);
				animator.SetBool("JumpRunning", true);
			}else{
				animator.SetBool("JumpRunning", false);
			}
		}
	}


		
	//a callback for calculating IK
	void OnAnimatorIK()
	{
		stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if(animator) {

			//if the IK is active, set the position and rotation directly to the goal. 
			if(stateInfo.IsName("Base Layer.Climb")) {
				if(getCurrentSide() == 1 && topCollider.x - animator.GetIKPosition(AvatarIKGoal.RightHand).x > 0 && distanceCorrection == 0){
					distanceCorrection = transform.position.x + Mathf.Abs(topCollider.x - animator.GetIKPosition(AvatarIKGoal.RightHand).x );
				}else if(getCurrentSide() == -1 && topCollider.x - animator.GetIKPosition(AvatarIKGoal.RightHand).x < 0 && distanceCorrection == 0){
					distanceCorrection = transform.position.x - Mathf.Abs(topCollider.x - animator.GetIKPosition(AvatarIKGoal.RightHand).x );
				}
				if(animator.GetIKPosition(AvatarIKGoal.RightHand).y > topCollider.y){
					animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
					animator.SetIKPosition(AvatarIKGoal.RightHand, new Vector3(animator.GetIKPosition(AvatarIKGoal.RightHand).x,topCollider.y,animator.GetIKPosition(AvatarIKGoal.RightHand).z));
				}
				if(animator.GetIKPosition(AvatarIKGoal.LeftHand).y > topCollider.y){
					animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,1);
					animator.SetIKPosition(AvatarIKGoal.LeftHand, new Vector3(animator.GetIKPosition(AvatarIKGoal.LeftHand).x,topCollider.y,animator.GetIKPosition(AvatarIKGoal.LeftHand).z));
				}

			}
			if(stateInfo.IsName("Base Layer.OverWall")) {
				if(animator.GetIKPosition(AvatarIKGoal.RightHand).y > topCollider.y && animator.GetFloat("HandCorrection") > 0){
					animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
					animator.SetIKPosition(AvatarIKGoal.RightHand, new Vector3(topCollider.x,topCollider.y,animator.GetIKPosition(AvatarIKGoal.RightHand).z));
				}
				if(animator.GetIKPosition(AvatarIKGoal.LeftHand).y > topCollider.y && animator.GetFloat("HandCorrection") > 0){
					animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,1);
					animator.SetIKPosition(AvatarIKGoal.LeftHand, new Vector3(topCollider.x,topCollider.y,animator.GetIKPosition(AvatarIKGoal.LeftHand).z));
				}

			}

			//if the IK is not active, set the position and rotation of the hand and head back to the original position
			if(!stateInfo.IsName("Base Layer.Climb") && !stateInfo.IsName("Base Layer.OverWall")) {          
				animator.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
				animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,0);
				animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,0);
				animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,0);
			}
		}
	}    

	private int getCurrentSide(){
		return (int) (transform.rotation.y / Mathf.Abs(transform.rotation.y)) * 1;
	}

	private void getTopCollider(RaycastHit hit){
		float height = hit.transform.localScale.y * ((BoxCollider)hit.collider).size.y;
		float widht = hit.transform.localScale.x * ((BoxCollider)hit.collider).size.x;
		topCollider = new Vector3(hit.point.x, hit.transform.position.y + ((BoxCollider)hit.collider).center.y + height / 2, transform.position.z);
	}

	private float currentVelocity(Vector3 oldPos){
		Vector3 nonCorrectedVelocity;

		nonCorrectedVelocity = (transform.position - oldPos) / Time.deltaTime;

		frameVelocity = Vector3.Lerp (frameVelocity, nonCorrectedVelocity, 0.1f);
		/*if (correctionVelocityY == 0 && velocityY == 0) {
			oldVelocityY = 0;
			correctionVelocityY = 1f;
		}

		if(velocityY == 0 && oldVelocityY != 0){
			//correctionVelocityY = velocityY;
			velocityY = oldVelocityY;
		}

		if (correctionVelocityY == velocityY) {
			velocityY += -1f;
		}*/

		return frameVelocity.y;
	}

	private float currentAcceleration(Vector3 oldVelocity){
		Vector3 nonCorrectedAcceleration;

		nonCorrectedAcceleration = (new Vector3(0f,velocityY) - oldVelocity) / Time.deltaTime;

		frameAcceleration = Vector3.Lerp (frameVelocity, nonCorrectedAcceleration, 0.1f);


		return frameAcceleration.y;
	}
}
