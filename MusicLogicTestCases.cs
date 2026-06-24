using System;
using System.Linq;
using System.Threading.Tasks;
using Grammophone.DataAccess.QueryExtensions;
using Grammophone.Domos.Logic;
using Grammophone.Domos.Tests.Music.Domain;
using Grammophone.Domos.Tests.Music.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grammophone.Domos.Tests.Music.Cases
{
	/// <summary>
	/// Shared music logic test cases.
	/// </summary>
	[TestClass]
	public abstract class MusicLogicTestCases
	{
		#region Test setup

		/// <summary>
		/// Reset provider data and seed the music test data.
		/// </summary>
		[TestInitialize]
		public void Initialize()
		{
			ResetData();
		}

		#endregion

		#region Protected properties

		/// <summary>
		/// The Unity configuration section name used by the test provider.
		/// </summary>
		protected abstract string ConfigurationSectionName { get; }

		#endregion

		#region Public tests

		/// <summary>
		/// Administrator can obtain all managers.
		/// </summary>
		[TestMethod]
		public void Administrator_can_obtain_all_managers()
		{
			using (var session = CreateSession(MusicTestData.AdminUserName))
			{
				var label = GetLabelA();

				Assert.IsNotNull(session.GetCatalogManager());
				Assert.IsNotNull(session.GetRecordLabelsManager());
				Assert.IsNotNull(session.GetArtistsManager(label));
				Assert.IsNotNull(session.GetAlbumsManager(label));
				Assert.IsNotNull(session.GetTracksManager(label));
			}
		}

		/// <summary>
		/// Catalog readers cannot obtain mutation managers.
		/// </summary>
		[TestMethod]
		public void Catalog_reader_cannot_obtain_label_scoped_mutation_manager()
		{
			using (var session = CreateSession(MusicTestData.ReaderUserName))
			{
				var label = GetLabelA();

				Assert.IsNotNull(session.GetCatalogManager());
				Assert.ThrowsException<ManagerAccessDeniedException>(() => session.GetAlbumsManager(label));
			}
		}

		/// <summary>
		/// Record label administrators can approve albums in their label.
		/// </summary>
		[TestMethod]
		public async Task Label_administrator_can_approve_album_in_own_label()
		{
			using (var session = CreateSession(MusicTestData.LabelAAdminUserName))
			{
				var label = GetLabelA();
				var manager = session.GetAlbumsManager(label);
				var album = await manager.Albums.SingleAsync(a => a.Title == MusicTestData.ReviewAlbumTitle);

				await manager.ApproveForReleaseAsync(album, DateTime.UtcNow.Date);

				Assert.AreEqual(AlbumWorkflowNames.Published, album.State.CodeName);
			}
		}

		/// <summary>
		/// Contributors cannot approve albums.
		/// </summary>
		[TestMethod]
		public async Task Contributor_cannot_approve_album()
		{
			using (var session = CreateSession(MusicTestData.LabelAContributorUserName))
			{
				var label = GetLabelA();
				var manager = session.GetAlbumsManager(label);
				var album = await manager.Albums.SingleAsync(a => a.Title == MusicTestData.ReviewAlbumTitle);

				await Assert.ThrowsExceptionAsync<StatePathAccessDeniedException>(
					async () => await manager.ApproveForReleaseAsync(album, DateTime.UtcNow.Date));
			}
		}

		/// <summary>
		/// Impersonation changes manager access decisions.
		/// </summary>
		[TestMethod]
		public async Task Impersonation_changes_manager_access_decisions()
		{
			using (var session = CreateSession(MusicTestData.AdminUserName))
			{
				var label = GetLabelA();

				Assert.IsNotNull(session.GetAlbumsManager(label));

				using (await session.GetTestImpersonationScopeAsync(u => u.UserName == MusicTestData.ReaderUserName))
				{
					Assert.ThrowsException<ManagerAccessDeniedException>(() => session.GetAlbumsManager(label));
				}
			}
		}

		#endregion

		#region Protected methods

		/// <summary>
		/// Reset and seed provider data.
		/// </summary>
		protected abstract void ResetData();

		#endregion

		#region Private methods

		private MusicSession CreateSession(string userName)
		{
			return new MusicSession(this.ConfigurationSectionName, user => user.UserName == userName);
		}

		private RecordLabel GetLabelA()
		{
			using (var adminSession = CreateSession(MusicTestData.AdminUserName))
			{
				return adminSession.GetCatalogManager().RecordLabels.Single(label => label.Name == MusicTestData.LabelAName);
			}
		}

		#endregion
	}
}
