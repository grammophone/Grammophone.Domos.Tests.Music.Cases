using System;
using System.Linq;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Domain.Workflow;
using Grammophone.Domos.Tests.Music.DataAccess;
using Grammophone.Domos.Tests.Music.Domain;
using Grammophone.Domos.Tests.Music.Logic;

namespace Grammophone.Domos.Tests.Music.Cases
{
	/// <summary>
	/// Seeds test data for the music Domos test application.
	/// </summary>
	public static class MusicTestDataSeeder
	{
		/// <summary>
		/// Seed all test data.
		/// </summary>
		public static void Seed(IMusicDomosDomainContainer domainContainer)
		{
			var adminRole = AddRole(domainContainer, MusicRoleNames.Administrator);
			var readerRole = AddRole(domainContainer, MusicRoleNames.CatalogReader);
			var authenticatedRole = AddRole(domainContainer, MusicRoleNames.Authenticated);

			var labelAdminType = AddDispositionType(domainContainer, MusicDispositionTypeNames.RecordLabelAdministrator, typeof(RecordLabelAdministrator));
			var contributorType = AddDispositionType(domainContainer, MusicDispositionTypeNames.RecordLabelContributor, typeof(RecordLabelContributor));

			var admin = AddUser(domainContainer, MusicTestData.AdminUserName, adminRole);
			AddUser(domainContainer, MusicTestData.ReaderUserName, readerRole);
			var labelAAdmin = AddUser(domainContainer, MusicTestData.LabelAAdminUserName, authenticatedRole);
			var labelAContributor = AddUser(domainContainer, MusicTestData.LabelAContributorUserName, authenticatedRole);
			var labelBAdmin = AddUser(domainContainer, MusicTestData.LabelBAdminUserName, authenticatedRole);
			AddUser(domainContainer, MusicTestData.OutsiderUserName, authenticatedRole);

			domainContainer.SaveChanges();

			var labelA = AddLabel(domainContainer, MusicTestData.LabelAName, admin);
			var labelB = AddLabel(domainContainer, MusicTestData.LabelBName, admin);

			domainContainer.SaveChanges();

			AddDisposition(domainContainer, labelAAdmin, labelA, labelAdminType, new RecordLabelAdministrator());
			AddDisposition(domainContainer, labelAContributor, labelA, contributorType, new RecordLabelContributor());
			AddDisposition(domainContainer, labelBAdmin, labelB, labelAdminType, new RecordLabelAdministrator());

			domainContainer.SaveChanges();

			var states = AddWorkflow(domainContainer);

			domainContainer.SaveChanges();

			var artist = AddArtist(domainContainer, labelA, "Test Artist");

			domainContainer.SaveChanges();

			AddAlbum(domainContainer, labelA, artist, labelAContributor, states.ReviewPending, MusicTestData.ReviewAlbumTitle);

			domainContainer.SaveChanges();
		}

		private static Role AddRole(IMusicDomosDomainContainer domainContainer, string codeName)
		{
			var role = new Role();
			role.CodeName = codeName;
			role.Name = codeName;
			domainContainer.Roles.Add(role);
			return role;
		}

		private static DispositionType AddDispositionType(IMusicDomosDomainContainer domainContainer, string codeName, Type type)
		{
			var dispositionType = new DispositionType();
			dispositionType.CodeName = codeName;
			dispositionType.Name = codeName;
			dispositionType.ClassName = type.FullName;
			domainContainer.DispositionTypes.Add(dispositionType);
			return dispositionType;
		}

		private static MusicUser AddUser(IMusicDomosDomainContainer domainContainer, string userName, Role role = null)
		{
			var user = new MusicUser();
			user.UserName = userName;
			user.Email = $"{userName}@music.test";
			user.FirstName = userName;
			user.LastName = "User";
			user.SecurityStamp = Guid.NewGuid().ToString("N");
			user.CreationDate = DateTime.UtcNow;
			if (role != null) user.Roles.Add(role);
			domainContainer.Users.Add(user);
			return user;
		}

		private static RecordLabel AddLabel(IMusicDomosDomainContainer domainContainer, string name, MusicUser creator)
		{
			var label = new RecordLabel();
			label.Name = name;
			label.OwningUser = creator;
			label.OwningUserID = creator.ID;
			label.SetCreator(creator, DateTime.UtcNow);
			label.RecordChange(creator, DateTime.UtcNow);
			domainContainer.RecordLabels.Add(label);
			return label;
		}

		private static void AddDisposition(IMusicDomosDomainContainer domainContainer, MusicUser user, RecordLabel label, DispositionType type, RecordLabelDisposition disposition)
		{
			disposition.User = user;
			disposition.RecordLabel = label;
			disposition.RecordLabelID = label.ID;
			disposition.Type = type;
			disposition.TypeID = type.ID;
			disposition.Status = DispositionStatus.Verified;
			disposition.SetCreator(user, DateTime.UtcNow);
			disposition.RecordChange(user, DateTime.UtcNow);
			domainContainer.Dispositions.Add(disposition);
		}

		private static Artist AddArtist(IMusicDomosDomainContainer domainContainer, RecordLabel label, string name)
		{
			var artist = new Artist();
			artist.Name = name;
			artist.RecordLabel = label;
			artist.RecordLabelID = label.ID;
			domainContainer.Artists.Add(artist);
			return artist;
		}

		private static Album AddAlbum(IMusicDomosDomainContainer domainContainer, RecordLabel label, Artist artist, MusicUser owner, State state, string title)
		{
			var album = new Album();
			album.Title = title;
			album.RecordLabel = label;
			album.RecordLabelID = label.ID;
			album.Artist = artist;
			album.ArtistID = artist.ID;
			album.Owner = owner;
			album.OwnerID = owner.ID;
			album.State = state;
			album.StateID = state.ID;
			album.SetCreator(owner, DateTime.UtcNow);
			album.RecordChange(owner, DateTime.UtcNow);
			domainContainer.Albums.Add(album);
			return album;
		}

		private static (State Draft, State ReviewPending, State Published, State Rejected, State Archived) AddWorkflow(IMusicDomosDomainContainer domainContainer)
		{
			var graph = new WorkflowGraph();
			graph.CodeName = AlbumWorkflowNames.Graph;
			graph.Name = AlbumWorkflowNames.Graph;
			graph.StateTransitionTypeName = typeof(AlbumStateTransition).FullName;
			domainContainer.WorkflowGraphs.Add(graph);

			var draftGroup = AddGroup(domainContainer, graph, "Drafting");
			var reviewGroup = AddGroup(domainContainer, graph, "Reviewing");
			var publishedGroup = AddGroup(domainContainer, graph, "PublishedGroup");
			var closedGroup = AddGroup(domainContainer, graph, "Closed");

			var draft = AddState(domainContainer, draftGroup, AlbumWorkflowNames.Draft, isStart: true);
			var reviewPending = AddState(domainContainer, reviewGroup, AlbumWorkflowNames.ReviewPending);
			var published = AddState(domainContainer, publishedGroup, AlbumWorkflowNames.Published);
			var rejected = AddState(domainContainer, closedGroup, AlbumWorkflowNames.Rejected, isEnd: true);
			var archived = AddState(domainContainer, closedGroup, AlbumWorkflowNames.Archived, isEnd: true);

			AddPath(domainContainer, graph, draft, reviewPending, AlbumWorkflowNames.SubmitForReview);
			AddPath(domainContainer, graph, reviewPending, published, AlbumWorkflowNames.ApproveForRelease);
			AddPath(domainContainer, graph, reviewPending, rejected, AlbumWorkflowNames.RejectRelease);
			AddPath(domainContainer, graph, published, archived, AlbumWorkflowNames.ArchiveAlbum);

			return (draft, reviewPending, published, rejected, archived);
		}

		private static StateGroup AddGroup(IMusicDomosDomainContainer domainContainer, WorkflowGraph graph, string codeName)
		{
			var group = new StateGroup();
			group.WorkflowGraph = graph;
			group.CodeName = codeName;
			group.Name = codeName;
			domainContainer.StateGroups.Add(group);
			return group;
		}

		private static State AddState(IMusicDomosDomainContainer domainContainer, StateGroup group, string codeName, bool isStart = false, bool isEnd = false)
		{
			var state = new State();
			state.Group = group;
			state.CodeName = codeName;
			state.Name = codeName;
			state.IsStart = isStart;
			state.IsEnd = isEnd;
			domainContainer.States.Add(state);
			return state;
		}

		private static void AddPath(IMusicDomosDomainContainer domainContainer, WorkflowGraph graph, State previous, State next, string codeName)
		{
			var path = new StatePath();
			path.WorkflowGraph = graph;
			path.PreviousState = previous;
			path.NextState = next;
			path.CodeName = codeName;
			path.Name = codeName;
			path.ChangeStampANDMask = -1;
			domainContainer.StatePaths.Add(path);
		}
	}
}
