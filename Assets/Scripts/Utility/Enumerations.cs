using UnityEngine;

// SECTION - Enumeration ============================================================

// Basic Ennemy
public enum BasicEnemy_States { ONE, TWO }

public enum BasicEnemy_AnimationStates { IDDLE, MOVEMENT, ONAWAKE, DEAD, STATE_01_TRANSITION, STATE_01_NOTOKEN, STATE_01_TOKEN, STATE_02_TRANSITION, STATE_02_NOTOKEN, STATE_02_TOKEN }

public enum BasicEnemy_AnimTriggers { DEATH, EXITDEATH, STATE_01_TRANSITION, STATE_01_NOTOKEN, STATE_01_TOKEN, STATE_02_TRANSITION, STATE_02_NOTOKEN, STATE_02_TOKEN}


// Base Enemy AI
public enum ValidationCheckTypes { CHILDSPECIFIC, ALWAYSVALID, RAYCASTSINGLE, RAYCASTARRAY, OVERLAPSPHERE }
