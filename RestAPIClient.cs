using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace AzureJobScheduler
{
    /// <summary>
    /// Client for interacting with the REST API.
    /// </summary>
    internal class RestAPIClient : IRestAPIClient
    {
        private string? _token;
        static string? api = string.Empty;
        static IConfiguration _config;
        static ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestAPIClient"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="logger">The logger instance.</param>
        public RestAPIClient(IConfiguration configuration, ILogger<BackgroundJob> logger)
        {
            _config = configuration;
            _logger = logger;
            api = _config["YOUR_RESTAPI_URL"];
            _logger.LogInformation($"YOUR_RESTAPI_URL: {api ?? ""}");
            if (string.IsNullOrEmpty(api)) { throw new NullReferenceException("YOUR_RESTAPI_URL value is invalid!"); }
        }

        /// <summary>
        /// Authenticates with the REST API and retrieves the authentication token.
        /// </summary>
        /// <returns>The authentication token.</returns>
        public async Task<string> Authenticate()
        {
            HttpClient client = new HttpClient();
            string endpoint = api + "/api/Auth/ServiceLogin"; // POST
            try
            {
                // Convert the request body to JSON
                string jsonRequestBody = JsonConvert.SerializeObject(
                    new Dictionary<string, string>
                {
                        { "UserName", _config["YOUR_RESTAPI_USERNAME"]},
                        { "Password", _config["YOUR_RESTAPI_PASSWORD"]}
                });

                // Set the Content-Type header to application/json
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage postResponse = await client.PostAsync(endpoint, new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
                postResponse.EnsureSuccessStatusCode(); // Ensure the POST API call was successful

                // Read the response content
                string postResponseBody = await postResponse.Content.ReadAsStringAsync();

                // Deserialize the JSON response to an AuthResponse object
                AuthResponse authResponse = JsonConvert.DeserializeObject<AuthResponse>(postResponseBody);

                // Save token for later use
                _token = authResponse.Token;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while making the API call: " + ex.Message);
            }
            finally
            {
                // Dispose of the HttpClient instance
                client.Dispose();
            }
            return _token;
        }

        /// <summary>
        /// Fetches the job details for the specified job ID.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        public async Task FetchJob(long jobId)
        {
            _token = await Authenticate();

            if (string.IsNullOrWhiteSpace(_token))
                throw new Exception("Auth token not set.");

            if (jobId < 0)
            {
                throw new Exception("invalid Job Id.");
            }

            HttpClient client = new HttpClient();
            string endpoint = api + $"/api/ImportJobs/Job/{jobId}"; // GET

            // Set the bearer token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            try
            {
                // Set the Content-Type header to application/json
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage getResponse = await client.GetAsync(endpoint);
                getResponse.EnsureSuccessStatusCode(); // Ensure the GET API call was successful

                // Read the response content
                string responseBody = await getResponse.Content.ReadAsStringAsync();

                // Deserialize the JSON response to an ImportJobDTO object
                ImportJobDTO response = JsonConvert.DeserializeObject<ImportJobDTO>(responseBody);
                var _jobDTO = response;

                Console.WriteLine("set jobDTO");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while making the API call: " + ex.Message);
            }
            finally
            {
                // Dispose of the HttpClient instance
                client.Dispose();
            }
        }

        /// <summary>
        /// Fetches the job schedules for the specified client ID.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <returns>A list of job schedules.</returns>
        public async Task<List<JobScheduleDTO>> FetchSchedulesAsync(int clientId)
        {
            _token = await Authenticate();

            if (string.IsNullOrWhiteSpace(_token))
                throw new Exception("Auth token not set.");

            if (clientId < 0)
            {
                throw new Exception("invalid Job Id.");
            }

            HttpClient client = new HttpClient();
            string endpoint = api + $"/api/ImportJobs/Schedules/{clientId}"; // GET

            // Set the bearer token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            try
            {
                // Set the Content-Type header to application/json
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage getResponse = await client.GetAsync(endpoint);
                getResponse.EnsureSuccessStatusCode(); // Ensure the GET API call was successful

                // Read the response content
                string responseBody = await getResponse.Content.ReadAsStringAsync();

                // Deserialize the JSON response to a list of JobScheduleDTO objects
                List<JobScheduleDTO> response = JsonConvert.DeserializeObject<List<JobScheduleDTO>>(responseBody);
                var schedulesDTO = response;
                return schedulesDTO;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while making the API call: " + ex.Message);
                throw ex;
            }
            finally
            {
                // Dispose of the HttpClient instance
                client.Dispose();
            }
        }

        /// <summary>
        /// Fetches all job schedules.
        /// </summary>
        /// <returns>A list of job schedules.</returns>
        public async Task<List<JobScheduleDTO>> FetchSchedulesAsync()
        {
            _token = await Authenticate();

            if (string.IsNullOrWhiteSpace(_token))
                throw new Exception("Auth token not set.");

            HttpClient client = new HttpClient();
            string endpoint = api + $"/api/ImportJobs/Schedules/"; // GET

            // Set the bearer token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            try
            {
                // Set the Content-Type header to application/json
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage getResponse = await client.GetAsync(endpoint);
                getResponse.EnsureSuccessStatusCode(); // Ensure the GET API call was successful

                // Read the response content
                string responseBody = await getResponse.Content.ReadAsStringAsync();

                // Deserialize the JSON response to a list of JobScheduleDTO objects
                List<JobScheduleDTO> response = JsonConvert.DeserializeObject<List<JobScheduleDTO>>(responseBody);
                var schedulesDTO = response;
                return schedulesDTO;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while making the API call: " + ex.Message);
                throw ex;
            }
            finally
            {
                // Dispose of the HttpClient instance
                client.Dispose();
            }
        }

        /// <summary>
        /// Runs the jobs for the specified schedule ID.
        /// </summary>
        /// <param name="scheduleId">The schedule ID.</param>
        public async Task RunJobs(long scheduleId)
        {
            _token = await Authenticate();

            if (string.IsNullOrWhiteSpace(_token))
                throw new Exception("Auth token not set.");

            if (scheduleId <= 0)
            {
                throw new Exception("invalid Schedule Id.");
            }

            HttpClient client = new HttpClient();
            string endpoint = api + $"/api/ImportJobs/Schedules/RunJobs/{scheduleId}"; // GET

            // Set the bearer token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            try
            {
                // Set the Content-Type header to application/json
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage getResponse = await client.PostAsync(endpoint, JsonContent.Create(scheduleId));
                getResponse.EnsureSuccessStatusCode(); // Ensure the GET API call was successful

                Console.WriteLine("set jobDTO");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while making the API call: " + ex.Message);
                throw;
            }
            finally
            {
                // Dispose of the HttpClient instance
                client.Dispose();
            }
        }
    }

    /// <summary>
    /// Represents the authentication response.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Gets or sets the authentication token.
        /// </summary>
        public string Token { get; set; } = "";

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        public string RefreshToken { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether the EULA is accepted.
        /// </summary>
        public bool IsEULAaccepted { get; set; }
    }

    /// <summary>
    /// Represents the import job details.
    /// </summary>
    public partial class ImportJobDTO
    {
        /// <summary>
        /// Gets or sets the job ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the job name.
        /// </summary>
        public string Name { get; set; }

        public long ClientId { get; set; }
        public long CompanyId { get; set; }
        public byte? GroupTypeId { get; set; }
        public byte ImportType { get; set; }
        public bool PublishMembership { get; set; }
        public bool AddRecords { get; set; }
        public bool UpdateRecords { get; set; }
        public bool RemoveRecords { get; set; }
        public bool SyncData { get; set; }
        public int? ActionTypeId { get; set; }
        public int? CallTypeId { get; set; }
        public long? NotificationPreferenceId { get; set; }
        public int? CardStateId { get; set; }
        public int JobScheduleId { get; set; }
        public byte RowStateId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long? ModifiedBy { get; set; }
    }

    /// <summary>
    /// Represents the job schedule details.
    /// </summary>
    public partial class JobScheduleDTO
    {
        /// <summary>
        /// Gets or sets the schedule ID.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the schedule name.
        /// </summary>
        public string Name { get; set; }

        public long ClientId { get; set; }
        public bool? Active { get; set; }
        public TimeSpan? StartTime { get; set; }
        public int? TimeZoneId { get; set; }
        public byte RecurrenceType { get; set; }
        public bool? Monday { get; set; }
        public bool? Tuesday { get; set; }
        public bool? Wednesday { get; set; }
        public bool? Thursday { get; set; }
        public bool? Friday { get; set; }
        public bool? Saturday { get; set; }
        public bool? Sunday { get; set; }
        public byte? DayOfMonth { get; set; }
        public DateTime? RecurrenceStartDate { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public byte RowStateId { get; set; }
        public DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long? ModifiedBy { get; set; }
        public virtual ICollection<ImportJobDTO> ImportJob { get; set; }
    }
}
