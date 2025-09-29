using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastBreath.AI
{
    /// <summary>
    /// Job system for NPCs with schedules and task management
    /// </summary>
    public class NPCJobSystem : MonoBehaviour
    {
        [Header("Job Configuration")]
        [SerializeField] private NPCJob assignedJob = NPCJob.None;
        [SerializeField] private float workDuration = 300f; // 5 minutes default
        [SerializeField] private float breakDuration = 60f; // 1 minute break
        [SerializeField] private bool useSchedule = true;
        
        [Header("Work Areas")]
        [SerializeField] private Transform workArea;
        [SerializeField] private float workRadius = 5f;
        [SerializeField] private LayerMask workTargetLayer = -1;
        
        [Header("Schedule")]
        [SerializeField] private JobSchedule[] dailySchedule;
        
        // Events
        public event Action<NPCJob> OnJobStarted;
        public event Action<NPCJob> OnJobCompleted;
        public event Action OnBreakStarted;
        public event Action OnBreakEnded;
        
        // Private fields
        private NPCController npcController;
        private NPCNeeds npcNeeds;
        private Coroutine currentJobCoroutine;
        private bool isWorking = false;
        private bool onBreak = false;
        private float currentWorkTime = 0f;
        private float currentBreakTime = 0f;
        private int currentScheduleIndex = 0;
        
        // Job performance tracking
        private Dictionary<NPCJob, float> jobExperience = new Dictionary<NPCJob, float>();
        private Dictionary<NPCJob, int> jobCompletionCount = new Dictionary<NPCJob, int>();
        
        void Start()
        {
            npcController = GetComponent<NPCController>();
            npcNeeds = GetComponent<NPCNeeds>();
            
            InitializeJobSystem();
        }
        
        void InitializeJobSystem()
        {
            // Initialize job experience
            foreach (NPCJob job in Enum.GetValues(typeof(NPCJob)))
            {
                if (job != NPCJob.None)
                {
                    jobExperience[job] = 0f;
                    jobCompletionCount[job] = 0;
                }
            }
            
            // Set work area to home position if not assigned
            if (workArea == null && npcController != null)
            {
                GameObject workAreaGO = new GameObject($"{gameObject.name}_WorkArea");
                workAreaGO.transform.position = npcController.GetHomePosition();
                workArea = workAreaGO.transform;
            }
            
            // Start job system
            if (useSchedule && dailySchedule.Length > 0)
            {
                StartCoroutine(ScheduleManager());
            }
            else if (assignedJob != NPCJob.None)
            {
                AssignJob(assignedJob);
            }
        }
        
        /// <summary>
        /// Assign a job to the NPC
        /// </summary>
        public void AssignJob(NPCJob job)
        {
            if (currentJobCoroutine != null)
            {
                StopCoroutine(currentJobCoroutine);
            }
            
            assignedJob = job;
            
            if (job != NPCJob.None)
            {
                currentJobCoroutine = StartCoroutine(ExecuteJob(job));
                OnJobStarted?.Invoke(job);
            }
        }
        
        /// <summary>
        /// Execute assigned job
        /// </summary>
        IEnumerator ExecuteJob(NPCJob job)
        {
            isWorking = true;
            currentWorkTime = 0f;
            
            Debug.Log($"{npcController.npcName} started job: {job}");
            
            while (isWorking && currentWorkTime < workDuration)
            {
                // Check if NPC needs override job (critical needs)
                if (ShouldInterruptJob())
                {
                    yield return new WaitForSeconds(5f); // Wait before resuming
                    continue;
                }
                
                // Execute job-specific behavior
                yield return StartCoroutine(ExecuteJobBehavior(job));
                
                currentWorkTime += Time.deltaTime;
                
                // Gain experience
                jobExperience[job] += Time.deltaTime * 0.1f;
                
                yield return null;
            }
            
            // Job completed
            CompleteJob(job);
        }
        
        /// <summary>
        /// Execute specific job behavior
        /// </summary>
        IEnumerator ExecuteJobBehavior(NPCJob job)
        {
            switch (job)
            {
                case NPCJob.Farm:
                    yield return StartCoroutine(FarmBehavior());
                    break;
                    
                case NPCJob.Guard:
                    yield return StartCoroutine(GuardBehavior());
                    break;
                    
                case NPCJob.Craft:
                    yield return StartCoroutine(CraftBehavior());
                    break;
                    
                case NPCJob.Patrol:
                    yield return StartCoroutine(PatrolBehavior());
                    break;
                    
                case NPCJob.Scavenge:
                    yield return StartCoroutine(ScavengeBehavior());
                    break;
                    
                case NPCJob.Cook:
                    yield return StartCoroutine(CookBehavior());
                    break;
                    
                case NPCJob.Build:
                    yield return StartCoroutine(BuildBehavior());
                    break;
                    
                default:
                    yield return new WaitForSeconds(1f);
                    break;
            }
        }
        
        #region Job Behaviors
        
        IEnumerator FarmBehavior()
        {
            // Move to work area
            if (workArea != null)
            {
                yield return StartCoroutine(MoveToWorkArea());
            }
            
            // Simulate farming work
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Fatigue, -2f);
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Morale, 1f);
            
            yield return new WaitForSeconds(2f);
        }
        
        IEnumerator GuardBehavior()
        {
            // Look for threats in area
            Collider[] threats = Physics.OverlapSphere(transform.position, workRadius, workTargetLayer);
            
            if (threats.Length > 0)
            {
                // Alert behavior
                npcNeeds.ModifyNeed(NPCNeeds.NeedType.Fear, 5f);
                Debug.Log($"{npcController.npcName} detected threat while guarding!");
            }
            else
            {
                // Patrol around work area
                Vector3 patrolPoint = workArea.position + UnityEngine.Random.insideUnitSphere * workRadius;
                patrolPoint.y = transform.position.y;
                
                npcController.MoveTo(patrolPoint);
            }
            
            yield return new WaitForSeconds(3f);
        }
        
        IEnumerator CraftBehavior()
        {
            // Move to crafting area
            yield return StartCoroutine(MoveToWorkArea());
            
            // Simulate crafting
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Fatigue, -1f);
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Morale, 2f);
            
            yield return new WaitForSeconds(4f);
        }
        
        IEnumerator PatrolBehavior()
        {
            // Generate patrol points around work area
            Vector3 patrolPoint = workArea.position + UnityEngine.Random.insideUnitSphere * workRadius;
            patrolPoint.y = transform.position.y;
            
            npcController.MoveTo(patrolPoint);
            
            // Wait to reach destination
            yield return new WaitForSeconds(5f);
            
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Fatigue, -1f);
        }
        
        IEnumerator ScavengeBehavior()
        {
            // Look for scavenging targets
            Collider[] scavengeTargets = Physics.OverlapSphere(transform.position, workRadius, workTargetLayer);
            
            if (scavengeTargets.Length > 0)
            {
                Transform target = scavengeTargets[UnityEngine.Random.Range(0, scavengeTargets.Length)].transform;
                npcController.MoveTo(target.position);
                
                yield return new WaitForSeconds(3f);
                
                // Simulate scavenging
                npcNeeds.ModifyNeed(NPCNeeds.NeedType.Morale, 3f);
            }
            else
            {
                // Wander to find resources
                Vector3 wanderPoint = transform.position + UnityEngine.Random.insideUnitSphere * workRadius;
                wanderPoint.y = transform.position.y;
                npcController.MoveTo(wanderPoint);
            }
            
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Fatigue, -2f);
            yield return new WaitForSeconds(2f);
        }
        
        IEnumerator CookBehavior()
        {
            yield return StartCoroutine(MoveToWorkArea());
            
            // Simulate cooking
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Hunger, 5f); // Taste testing
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Morale, 2f);
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Fatigue, -1f);
            
            yield return new WaitForSeconds(3f);
        }
        
        IEnumerator BuildBehavior()
        {
            yield return StartCoroutine(MoveToWorkArea());
            
            // Simulate building work
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Fatigue, -3f);
            npcNeeds.ModifyNeed(NPCNeeds.NeedType.Morale, 1f);
            
            yield return new WaitForSeconds(5f);
        }
        
        #endregion
        
        /// <summary>
        /// Move to work area
        /// </summary>
        IEnumerator MoveToWorkArea()
        {
            if (workArea != null && npcController != null)
            {
                Vector3 workPosition = workArea.position + UnityEngine.Random.insideUnitSphere * 2f;
                workPosition.y = transform.position.y;
                
                npcController.MoveTo(workPosition);
                
                // Wait to reach work area
                yield return new WaitForSeconds(2f);
            }
        }
        
        /// <summary>
        /// Check if job should be interrupted for critical needs
        /// </summary>
        bool ShouldInterruptJob()
        {
            if (npcNeeds == null) return false;
            
            // Critical needs override job
            if (npcNeeds.GetNeed(NPCNeeds.NeedType.Health) < 30f) return true;
            if (npcNeeds.GetNeed(NPCNeeds.NeedType.Hunger) < 20f) return true;
            if (npcNeeds.GetNeed(NPCNeeds.NeedType.Fatigue) < 15f) return true;
            if (npcNeeds.GetNeed(NPCNeeds.NeedType.Fear) > 80f) return true;
            
            return false;
        }
        
        /// <summary>
        /// Complete current job
        /// </summary>
        void CompleteJob(NPCJob job)
        {
            isWorking = false;
            jobCompletionCount[job]++;
            
            Debug.Log($"{npcController.npcName} completed job: {job}");
            OnJobCompleted?.Invoke(job);
            
            // Start break
            if (breakDuration > 0f)
            {
                StartCoroutine(TakeBreak());
            }
        }
        
        /// <summary>
        /// Take a break between jobs
        /// </summary>
        IEnumerator TakeBreak()
        {
            onBreak = true;
            currentBreakTime = 0f;
            
            OnBreakStarted?.Invoke();
            Debug.Log($"{npcController.npcName} is taking a break");
            
            while (currentBreakTime < breakDuration)
            {
                // Rest during break
                npcNeeds.ModifyNeed(NPCNeeds.NeedType.Fatigue, 5f * Time.deltaTime);
                npcNeeds.ModifyNeed(NPCNeeds.NeedType.Morale, 2f * Time.deltaTime);
                
                currentBreakTime += Time.deltaTime;
                yield return null;
            }
            
            onBreak = false;
            OnBreakEnded?.Invoke();
            
            // Resume job or wait for next assignment
            if (useSchedule)
            {
                // Schedule manager will handle next job
            }
            else if (assignedJob != NPCJob.None)
            {
                AssignJob(assignedJob);
            }
        }
        
        /// <summary>
        /// Schedule manager for daily routines
        /// </summary>
        IEnumerator ScheduleManager()
        {
            while (true)
            {
                if (dailySchedule.Length > 0)
                {
                    JobSchedule currentSchedule = dailySchedule[currentScheduleIndex];
                    
                    // Execute scheduled job
                    AssignJob(currentSchedule.job);
                    
                    // Wait for job duration
                    yield return new WaitForSeconds(currentSchedule.duration);
                    
                    // Move to next schedule item
                    currentScheduleIndex = (currentScheduleIndex + 1) % dailySchedule.Length;
                }
                
                yield return new WaitForSeconds(1f);
            }
        }
        
        #region Public Interface
        
        public NPCJob GetCurrentJob() => assignedJob;
        public bool IsWorking() => isWorking;
        public bool IsOnBreak() => onBreak;
        public float GetJobExperience(NPCJob job) => jobExperience.ContainsKey(job) ? jobExperience[job] : 0f;
        public int GetJobCompletionCount(NPCJob job) => jobCompletionCount.ContainsKey(job) ? jobCompletionCount[job] : 0;
        
        public void SetWorkArea(Transform area) => workArea = area;
        public void SetWorkRadius(float radius) => workRadius = radius;
        
        #endregion
        
        void OnDrawGizmosSelected()
        {
            if (workArea != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(workArea.position, workRadius);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(workArea.position, 0.5f);
            }
        }
    }
    
    /// <summary>
    /// Job schedule entry for daily routines
    /// </summary>
    [System.Serializable]
    public class JobSchedule
    {
        public NPCJob job;
        public float duration = 300f; // 5 minutes default
        public string description;
    }
}