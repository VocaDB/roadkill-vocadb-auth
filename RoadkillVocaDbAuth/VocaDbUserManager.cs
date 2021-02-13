using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace VocaDb.RoadkillAuth
{
	public enum UserGroupId
	{
		Nothing,

		Limited,

		Regular,

		Trusted,

		Moderator,

		Admin,
	}

	public class UserForApiContract
	{
		[DataMember]
		public UserGroupId GroupId { get; set; }
	}

	public class PartialFindResult<T>
	{
		public PartialFindResult()
		{
			Items = new T[] { };
		}

		public PartialFindResult(T[] items, int totalCount)
		{
			Items = items;
			TotalCount = totalCount;
		}

		public PartialFindResult(T[] items, int totalCount, string term)
			: this(items, totalCount)
		{
			Term = term;
		}

		[DataMember]
		public T[] Items { get; set; }

		[DataMember]
		public string Term { get; set; }

		[DataMember]
		public int TotalCount { get; set; }
	}

	public class VocaDbUserManager : UserServiceBase
	{
		private UserForApiContract GetContractOrDefault(string username)
		{
			using (var client = new HttpClient())
			{
				// Code from: https://docs.microsoft.com/en-us/archive/blogs/jpsanders/asp-net-do-not-use-task-result-in-main-context
				var stringTask = Task.Run(() => client.GetStringAsync($"https://vocadb.net/api/users?query={username}&nameMatchMode=Exact&maxResults=1"));
				stringTask.Wait();
				var value = stringTask.Result;
				var users = JsonConvert.DeserializeObject<PartialFindResult<UserForApiContract>>(value);
				return users.Items.FirstOrDefault();
			}
		}

		private static bool IsAdmin(UserForApiContract contract)
		{
			return contract != null && contract.GroupId >= UserGroupId.Moderator;
		}

		private static bool IsEditor(UserForApiContract contract)
		{
			return contract != null && contract.GroupId >= UserGroupId.Trusted;
		}

		public VocaDbUserManager()
			: base(null, null) { }

		public VocaDbUserManager(ApplicationSettings configuration, IRepository repository)
			: base(configuration, repository) { }

		public override bool ActivateUser(string activationKey)
		{
			throw new NotSupportedException();
		}

		public override bool AddUser(string email, string username, string password, bool isAdmin, bool isEditor)
		{
			throw new NotSupportedException();
		}

		public override bool Authenticate(string email, string password)
		{
			throw new NotSupportedException("Not supported anymore, use the VocaDB login page");

			/*try {
			    using (var client = new QueryServiceClient()) {
					var contract = client.GetUser(email, password);
					if (contract != null) {
						if (FormsAuthentication.IsEnabled)
							FormsAuthentication.SetAuthCookie(email, true);

						return true;
					}

					return false;
				}
	    } catch (FaultException ex) {
				throw new SecurityException(ex, "An error occurred authentication user {0}", email);
			}*/
		}

		public override void ChangePassword(string email, string newPassword)
		{
			throw new NotSupportedException();
		}

		public override bool ChangePassword(string email, string oldPassword, string newPassword)
		{
			throw new NotSupportedException();
		}

		public override bool DeleteUser(string email)
		{
			throw new NotSupportedException();
		}

		public override string GetLoggedInUserName(HttpContextBase context)
		{
			if (FormsAuthentication.IsEnabled)
			{
				if (context.Request.Cookies[FormsAuthentication.FormsCookieName] != null)
				{
					string cookie = context.Request.Cookies[FormsAuthentication.FormsCookieName].Value;
					if (!string.IsNullOrEmpty(cookie))
					{
						FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie);
						return ticket.Name;
					}
				}
			}

			return "";
		}

		public override User GetUser(string email, bool? isActivated = true)
		{
			var contract = GetContractOrDefault(email);

			return new User
			{
				Email = email,
				Username = email,
				IsActivated = isActivated ?? true,
				IsEditor = IsEditor(contract),
				IsAdmin = IsAdmin(contract),
			};
		}

		public override User GetUserById(Guid id, bool? isActivated = null)
		{
			throw new NotImplementedException();
		}

		public override User GetUserByResetKey(string resetKey)
		{
			throw new NotSupportedException();
		}

		public override bool IsAdmin(string email)
		{
			var contract = GetContractOrDefault(email);
			return IsAdmin(contract);
		}

		public override bool IsEditor(string email)
		{
			var contract = GetContractOrDefault(email);
			return IsEditor(contract);
		}

		public override bool IsReadonly
		{
			get { return true; }
		}

		public override IEnumerable<UserViewModel> ListAdmins()
		{
			return Enumerable.Empty<UserViewModel>();
		}

		public override IEnumerable<UserViewModel> ListEditors()
		{
			return Enumerable.Empty<UserViewModel>();
		}

		public override void Logout()
		{
			if (FormsAuthentication.IsEnabled)
				FormsAuthentication.SignOut();
		}

		public override string ResetPassword(string email)
		{
			throw new NotSupportedException();
		}

		public override string Signup(UserViewModel summary, Action completed)
		{
			throw new NotSupportedException();
		}

		public override void ToggleAdmin(string email)
		{
			throw new NotSupportedException();
		}

		public override void ToggleEditor(string email)
		{
			throw new NotSupportedException();
		}

		public override bool UpdateUser(UserViewModel summary)
		{
			throw new NotSupportedException();
		}

		public override bool UserExists(string email)
		{
			return GetContractOrDefault(email) != null;
		}

		public override bool UserNameExists(string username)
		{
			return GetContractOrDefault(username) != null;
		}
	}
}
