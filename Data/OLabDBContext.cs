using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace OLabWebAPI.Model
{
    public partial class OLabDBContext : DbContext
    {
        public OLabDBContext()
        {
        }

        public OLabDBContext(DbContextOptions<OLabDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AuthorRights> AuthorRights { get; set; }
        public virtual DbSet<Cron> Cron { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<H5pContents> H5pContents { get; set; }
        public virtual DbSet<H5pContentsLibraries> H5pContentsLibraries { get; set; }
        public virtual DbSet<H5pContentsTags> H5pContentsTags { get; set; }
        public virtual DbSet<H5pContentsUserData> H5pContentsUserData { get; set; }
        public virtual DbSet<H5pCounters> H5pCounters { get; set; }
        public virtual DbSet<H5pEvents> H5pEvents { get; set; }
        public virtual DbSet<H5pLibraries> H5pLibraries { get; set; }
        public virtual DbSet<H5pLibrariesCachedassets> H5pLibrariesCachedassets { get; set; }
        public virtual DbSet<H5pLibrariesLanguages> H5pLibrariesLanguages { get; set; }
        public virtual DbSet<H5pLibrariesLibraries> H5pLibrariesLibraries { get; set; }
        public virtual DbSet<H5pResults> H5pResults { get; set; }
        public virtual DbSet<H5pTags> H5pTags { get; set; }
        public virtual DbSet<Languages> Languages { get; set; }
        public virtual DbSet<Lrs> Lrs { get; set; }
        public virtual DbSet<LrsStatement> LrsStatement { get; set; }
        public virtual DbSet<LtiConsumer> LtiConsumer { get; set; }
        public virtual DbSet<LtiConsumers> LtiConsumers { get; set; }
        public virtual DbSet<LtiContexts> LtiContexts { get; set; }
        public virtual DbSet<LtiNonces> LtiNonces { get; set; }
        public virtual DbSet<LtiProviders> LtiProviders { get; set; }
        public virtual DbSet<LtiSharekeys> LtiSharekeys { get; set; }
        public virtual DbSet<LtiUsers> LtiUsers { get; set; }
        public virtual DbSet<MapAvatars> MapAvatars { get; set; }
        public virtual DbSet<MapChatElements> MapChatElements { get; set; }
        public virtual DbSet<MapChats> MapChats { get; set; }
        public virtual DbSet<MapCollectionMaps> MapCollectionMaps { get; set; }
        public virtual DbSet<MapCollections> MapCollections { get; set; }
        public virtual DbSet<MapContributorRoles> MapContributorRoles { get; set; }
        public virtual DbSet<MapContributors> MapContributors { get; set; }
        public virtual DbSet<MapCounterCommonRules> MapCounterCommonRules { get; set; }
        public virtual DbSet<MapCounterRuleRelations> MapCounterRuleRelations { get; set; }
        public virtual DbSet<MapCounterRules> MapCounterRules { get; set; }
        public virtual DbSet<MapCounters> MapCounters { get; set; }
        public virtual DbSet<MapDamElements> MapDamElements { get; set; }
        public virtual DbSet<MapDams> MapDams { get; set; }
        public virtual DbSet<MapElements> MapElements { get; set; }
        public virtual DbSet<MapElementsMetadata> MapElementsMetadata { get; set; }
        public virtual DbSet<MapFeedbackOperators> MapFeedbackOperators { get; set; }
        public virtual DbSet<MapFeedbackRules> MapFeedbackRules { get; set; }
        public virtual DbSet<MapFeedbackTypes> MapFeedbackTypes { get; set; }
        public virtual DbSet<MapGroups> MapGroups { get; set; }
        public virtual DbSet<MapKeys> MapKeys { get; set; }
        public virtual DbSet<MapNodeCounters> MapNodeCounters { get; set; }
        public virtual DbSet<MapNodeJumps> MapNodeJumps { get; set; }
        public virtual DbSet<MapNodeLinkStylies> MapNodeLinkStylies { get; set; }
        public virtual DbSet<MapNodeLinkTypes> MapNodeLinkTypes { get; set; }
        public virtual DbSet<MapNodeLinks> MapNodeLinks { get; set; }
        public virtual DbSet<MapNodeNotes> MapNodeNotes { get; set; }
        public virtual DbSet<MapNodePriorities> MapNodePriorities { get; set; }
        public virtual DbSet<MapNodeReferences> MapNodeReferences { get; set; }
        public virtual DbSet<MapNodeSectionNodes> MapNodeSectionNodes { get; set; }
        public virtual DbSet<MapNodeSections> MapNodeSections { get; set; }
        public virtual DbSet<MapNodeTypes> MapNodeTypes { get; set; }
        public virtual DbSet<MapNodes> MapNodes { get; set; }
        public virtual DbSet<MapPopupAssignTypes> MapPopupAssignTypes { get; set; }
        public virtual DbSet<MapPopupPositionTypes> MapPopupPositionTypes { get; set; }
        public virtual DbSet<MapPopupPositions> MapPopupPositions { get; set; }
        public virtual DbSet<MapPopups> MapPopups { get; set; }
        public virtual DbSet<MapPopupsAssign> MapPopupsAssign { get; set; }
        public virtual DbSet<MapPopupsCounters> MapPopupsCounters { get; set; }
        public virtual DbSet<MapPopupsStyles> MapPopupsStyles { get; set; }
        public virtual DbSet<MapQuestionResponses> MapQuestionResponses { get; set; }
        public virtual DbSet<MapQuestionTypes> MapQuestionTypes { get; set; }
        public virtual DbSet<MapQuestionValidation> MapQuestionValidation { get; set; }
        public virtual DbSet<MapQuestions> MapQuestions { get; set; }
        public virtual DbSet<MapSections> MapSections { get; set; }
        public virtual DbSet<MapSecurities> MapSecurities { get; set; }
        public virtual DbSet<MapSkins> MapSkins { get; set; }
        public virtual DbSet<MapTypes> MapTypes { get; set; }
        public virtual DbSet<MapUsers> MapUsers { get; set; }
        public virtual DbSet<MapVpdElements> MapVpdElements { get; set; }
        public virtual DbSet<MapVpdTypes> MapVpdTypes { get; set; }
        public virtual DbSet<MapVpds> MapVpds { get; set; }
        public virtual DbSet<Maps> Maps { get; set; }
        public virtual DbSet<OauthProviders> OauthProviders { get; set; }
        public virtual DbSet<Options> Options { get; set; }
        public virtual DbSet<Phinxlog> Phinxlog { get; set; }
        public virtual DbSet<QCumulative> QCumulative { get; set; }
        public virtual DbSet<ScenarioMaps> ScenarioMaps { get; set; }
        public virtual DbSet<Scenarios> Scenarios { get; set; }
        public virtual DbSet<ScopeTypes> ScopeTypes { get; set; }
        public virtual DbSet<SecurityRoles> SecurityRoles { get; set; }
        public virtual DbSet<SecurityUsers> SecurityUsers { get; set; }
        public virtual DbSet<Servers> Servers { get; set; }
        public virtual DbSet<SjtResponse> SjtResponse { get; set; }
        public virtual DbSet<Statements> Statements { get; set; }
        public virtual DbSet<StatisticsUserDatesave> StatisticsUserDatesave { get; set; }
        public virtual DbSet<StatisticsUserResponses> StatisticsUserResponses { get; set; }
        public virtual DbSet<StatisticsUserSessions> StatisticsUserSessions { get; set; }
        public virtual DbSet<StatisticsUserSessionTraces> StatisticsUserSessionTraces { get; set; }
        public virtual DbSet<SystemConstants> SystemConstants { get; set; }
        public virtual DbSet<SystemCounterActions> SystemCounterActions { get; set; }
        public virtual DbSet<SystemCounters> SystemCounters { get; set; }
        public virtual DbSet<SystemCourses> SystemCourses { get; set; }
        public virtual DbSet<SystemFiles> SystemFiles { get; set; }
        public virtual DbSet<SystemGlobals> SystemGlobals { get; set; }
        public virtual DbSet<SystemQuestionResponses> SystemQuestionResponses { get; set; }
        public virtual DbSet<SystemQuestionTypes> SystemQuestionTypes { get; set; }
        public virtual DbSet<SystemQuestionValidation> SystemQuestionValidation { get; set; }
        public virtual DbSet<SystemQuestions> SystemQuestions { get; set; }
        public virtual DbSet<SystemScripts> SystemScripts { get; set; }
        public virtual DbSet<SystemServers> SystemServers { get; set; }
        public virtual DbSet<SystemSettings> SystemSettings { get; set; }
        public virtual DbSet<SystemThemes> SystemThemes { get; set; }
        public virtual DbSet<TodayTips> TodayTips { get; set; }
        public virtual DbSet<TwitterCredits> TwitterCredits { get; set; }
        public virtual DbSet<UserBookmarks> UserBookmarks { get; set; }
        public virtual DbSet<UserGroups> UserGroups { get; set; }
        public virtual DbSet<UserNotes> UserNotes { get; set; }
        public virtual DbSet<UserResponses> UserResponses { get; set; }
        public virtual DbSet<UserSessions> UserSessions { get; set; }
        public virtual DbSet<UserSessionTraces> UserSessionTraces { get; set; }
        public virtual DbSet<UserState> UserState { get; set; }
        public virtual DbSet<UserTypes> UserTypes { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Vocablets> Vocablets { get; set; }
        public virtual DbSet<WebinarGroups> WebinarGroups { get; set; }
        public virtual DbSet<WebinarMacros> WebinarMacros { get; set; }
        public virtual DbSet<WebinarMaps> WebinarMaps { get; set; }
        public virtual DbSet<WebinarNodePoll> WebinarNodePoll { get; set; }
        public virtual DbSet<WebinarPoll> WebinarPoll { get; set; }
        public virtual DbSet<WebinarSteps> WebinarSteps { get; set; }
        public virtual DbSet<WebinarUsers> WebinarUsers { get; set; }
        public virtual DbSet<Webinars> Webinars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            modelBuilder.Entity<Cron>(entity =>
            {
                entity.HasOne(d => d.Rule)
                    .WithMany(p => p.Cron)
                    .HasForeignKey(d => d.RuleId)
                    .HasConstraintName("cron_ibfk_1");
            });

            modelBuilder.Entity<H5pContents>(entity =>
            {
                entity.Property(e => e.Author)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.ContentType)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("'0000-00-00 00:00:00'");

                entity.Property(e => e.Description)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.EmbedType)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Filtered)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Keywords)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.License)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Parameters)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Slug)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Title)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("'0000-00-00 00:00:00'");
            });

            modelBuilder.Entity<H5pContentsLibraries>(entity =>
            {
                entity.HasKey(e => new { e.ContentId, e.LibraryId, e.DependencyType })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

                entity.Property(e => e.DependencyType)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");
            });

            modelBuilder.Entity<H5pContentsTags>(entity =>
            {
                entity.HasKey(e => new { e.ContentId, e.TagId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
            });

            modelBuilder.Entity<H5pContentsUserData>(entity =>
            {
                entity.HasKey(e => new { e.ContentId, e.UserId, e.SubContentId, e.DataId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0, 0 });

                entity.Property(e => e.DataId)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Data)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("'0000-00-00 00:00:00'");
            });

            modelBuilder.Entity<H5pCounters>(entity =>
            {
                entity.HasKey(e => new { e.Type, e.LibraryName, e.LibraryVersion })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

                entity.Property(e => e.Type)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.LibraryName)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.LibraryVersion)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");
            });

            modelBuilder.Entity<H5pEvents>(entity =>
            {
                entity.Property(e => e.ContentTitle)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.LibraryName)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.LibraryVersion)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.SubType)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Type)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");
            });

            modelBuilder.Entity<H5pLibraries>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.DropLibraryCss)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.EmbedTypes)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Name)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.PreloadedCss)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.PreloadedJs)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Semantics)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Title)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.TutorialUrl)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("'0000-00-00 00:00:00'");
            });

            modelBuilder.Entity<H5pLibrariesCachedassets>(entity =>
            {
                entity.HasKey(e => new { e.LibraryId, e.Hash })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.Property(e => e.Hash)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");
            });

            modelBuilder.Entity<H5pLibrariesLanguages>(entity =>
            {
                entity.HasKey(e => new { e.LibraryId, e.LanguageCode })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.Property(e => e.LanguageCode)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");

                entity.Property(e => e.Translation)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");
            });

            modelBuilder.Entity<H5pLibrariesLibraries>(entity =>
            {
                entity.HasKey(e => new { e.LibraryId, e.RequiredLibraryId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.Property(e => e.DependencyType)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");
            });

            modelBuilder.Entity<H5pTags>(entity =>
            {
                entity.Property(e => e.Name)
                    .UseCollation("utf8mb4_unicode_ci")
                    .HasCharSet("utf8mb4");
            });

            modelBuilder.Entity<LrsStatement>(entity =>
            {
                entity.HasOne(d => d.Lrs)
                    .WithMany(p => p.LrsStatement)
                    .HasForeignKey(d => d.LrsId)
                    .HasConstraintName("lrs_statement_ibfk_1");

                entity.HasOne(d => d.Statement)
                    .WithMany(p => p.LrsStatement)
                    .HasForeignKey(d => d.StatementId)
                    .HasConstraintName("lrs_statement_ibfk_2");
            });

            modelBuilder.Entity<LtiConsumer>(entity =>
            {
                entity.Property(e => e.Role).HasDefaultValueSql("'1'");
            });

            modelBuilder.Entity<LtiConsumers>(entity =>
            {
                entity.Property(e => e.Role).HasDefaultValueSql("'1'");
            });

            modelBuilder.Entity<LtiContexts>(entity =>
            {
                entity.HasKey(e => e.ConsumerKey)
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<LtiNonces>(entity =>
            {
                entity.HasKey(e => e.ConsumerKey)
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<LtiSharekeys>(entity =>
            {
                entity.HasKey(e => e.ShareKeyId)
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<LtiUsers>(entity =>
            {
                entity.HasKey(e => e.ConsumerKey)
                    .HasName("PRIMARY");

                entity.HasOne(d => d.ConsumerKeyNavigation)
                    .WithOne(p => p.LtiUsers)
                    .HasForeignKey<LtiUsers>(d => d.ConsumerKey)
                    .HasConstraintName("lti_users_ibfk_2");
            });

            modelBuilder.Entity<MapAvatars>(entity =>
            {
                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapAvatars)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_avatars_ibfk_1");
            });

            modelBuilder.Entity<MapChatElements>(entity =>
            {
                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.MapChatElements)
                    .HasForeignKey(d => d.ChatId)
                    .HasConstraintName("map_chat_elements_ibfk_1");
            });

            modelBuilder.Entity<MapChats>(entity =>
            {
                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapChats)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_chats_ibfk_1");
            });

            modelBuilder.Entity<MapCollectionMaps>(entity =>
            {
                entity.HasOne(d => d.Collection)
                    .WithMany(p => p.MapCollectionMaps)
                    .HasForeignKey(d => d.CollectionId)
                    .HasConstraintName("map_collectionmaps_ibfk_1");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapCollectionMaps)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_collectionmaps_ibfk_2");
            });

            modelBuilder.Entity<MapContributors>(entity =>
            {
                entity.Property(e => e.Order).HasDefaultValueSql("'1'");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapContributors)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_contributors_ibfk_1");
            });

            modelBuilder.Entity<MapCounterCommonRules>(entity =>
            {
                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapCounterCommonRules)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_counter_common_rules_ibfk_1");
            });

            modelBuilder.Entity<MapCounterRules>(entity =>
            {
                entity.HasOne(d => d.CounterNavigation)
                    .WithMany(p => p.MapCounterRules)
                    .HasForeignKey(d => d.CounterId)
                    .HasConstraintName("map_counter_rules_ibfk_1");
            });

            modelBuilder.Entity<MapCounters>(entity =>
            {
                entity.Property(e => e.Visible).HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapCounters)
                    .HasForeignKey(d => d.MapId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("map_counters_ibfk_1");
            });

            modelBuilder.Entity<MapDamElements>(entity =>
            {
                entity.Property(e => e.Order).HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Dam)
                    .WithMany(p => p.MapDamElements)
                    .HasForeignKey(d => d.DamId)
                    .HasConstraintName("map_dam_elements_ibfk_1");
            });

            modelBuilder.Entity<MapDams>(entity =>
            {
                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapDams)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_dams_ibfk_1");
            });

            modelBuilder.Entity<MapElements>(entity =>
            {
                entity.Property(e => e.HeightType).HasDefaultValueSql("'px'");

                entity.Property(e => e.IsShared).HasDefaultValueSql("'1'");

                entity.Property(e => e.WidthType).HasDefaultValueSql("'px'");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapElements)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_elements_ibfk_1");
            });

            modelBuilder.Entity<MapFeedbackRules>(entity =>
            {
                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapFeedbackRules)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_feedback_rules_ibfk_1");
            });

            modelBuilder.Entity<MapKeys>(entity =>
            {
                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapKeys)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_keys_ibfk_1");
            });

            modelBuilder.Entity<MapNodeCounters>(entity =>
            {
                entity.Property(e => e.Display).HasDefaultValueSql("'1'");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.MapNodeCounters)
                    .HasForeignKey(d => d.NodeId)
                    .HasConstraintName("map_node_counters_ibfk_1");
            });

            modelBuilder.Entity<MapNodeJumps>(entity =>
            {
                entity.Property(e => e.Hidden).HasDefaultValueSql("'0'");

                entity.Property(e => e.Order).HasDefaultValueSql("'1'");

                entity.Property(e => e.Probability).HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapNodeJumps)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_jumps_ibfk_1");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.MapNodeJumps)
                    .HasForeignKey(d => d.NodeId)
                    .HasConstraintName("map_jumps_ibfk_4");
            });

            modelBuilder.Entity<MapNodeLinks>(entity =>
            {
                entity.Property(e => e.Hidden).HasDefaultValueSql("'0'");

                entity.Property(e => e.Order).HasDefaultValueSql("'1'");

                entity.Property(e => e.Probability).HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapNodeLinks)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_node_links_ibfk_1");

                entity.HasOne(d => d.NodeId1Navigation)
                    .WithMany(p => p.MapNodeLinksNodeId1Navigation)
                    .HasForeignKey(d => d.NodeId1)
                    .HasConstraintName("map_node_links_ibfk_4");

                entity.HasOne(d => d.NodeId2Navigation)
                    .WithMany(p => p.MapNodeLinksNodeId2Navigation)
                    .HasForeignKey(d => d.NodeId2)
                    .HasConstraintName("map_node_links_ibfk_5");
            });

            modelBuilder.Entity<MapNodeNotes>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.MapNode)
                    .WithMany(p => p.MapNodeNotes)
                    .HasForeignKey(d => d.MapNodeId)
                    .HasConstraintName("fk_map_node");
            });

            modelBuilder.Entity<MapNodeSectionNodes>(entity =>
            {
                entity.HasOne(d => d.Node)
                    .WithMany(p => p.MapNodeSectionNodes)
                    .HasForeignKey(d => d.NodeId)
                    .HasConstraintName("map_node_section_nodes_ibfk_3");

                entity.HasOne(d => d.Section)
                    .WithMany(p => p.MapNodeSectionNodes)
                    .HasForeignKey(d => d.SectionId)
                    .HasConstraintName("map_node_section_nodes_ibfk_1");
            });

            modelBuilder.Entity<MapNodeSections>(entity =>
            {
                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapNodeSections)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_node_sections_ibfk_1");
            });

            modelBuilder.Entity<MapNodes>(entity =>
            {
                entity.Property(e => e.ForceReload).HasDefaultValueSql("'0'");

                entity.Property(e => e.LinkTypeId).HasDefaultValueSql("'1'");

                entity.HasOne(d => d.LinkStyle)
                    .WithMany(p => p.MapNodes)
                    .HasForeignKey(d => d.LinkStyleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("map_nodes_ibfk_2");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapNodes)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_nodes_ibfk_1");
            });

            modelBuilder.Entity<MapPopupsAssign>(entity =>
            {
                entity.Property(e => e.RedirectTypeId).HasDefaultValueSql("'1'");
            });

            modelBuilder.Entity<MapPopupsCounters>(entity =>
            {
                entity.HasOne(d => d.Popup)
                    .WithMany(p => p.MapPopupsCounters)
                    .HasForeignKey(d => d.PopupId)
                    .HasConstraintName("map_popups_counters_ibfk_1");
            });

            modelBuilder.Entity<MapPopupsStyles>(entity =>
            {
                entity.Property(e => e.IsDefaultBackgroundColor).HasDefaultValueSql("'1'");
            });

            modelBuilder.Entity<MapQuestionResponses>(entity =>
            {
                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("map_question_responses_ibfk_2");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.MapQuestionResponses)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("map_question_responses_ibfk_1");
            });

            modelBuilder.Entity<MapQuestionValidation>(entity =>
            {
                entity.HasOne(d => d.Question)
                    .WithMany(p => p.MapQuestionValidation)
                    .HasForeignKey(d => d.QuestionId)
                    .HasConstraintName("map_question_validation_ibfk_1");
            });

            modelBuilder.Entity<MapQuestions>(entity =>
            {
                entity.Property(e => e.NumTries).HasDefaultValueSql("-1");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapQuestions)
                    .HasForeignKey(d => d.MapId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("map_questions_ibfk_1");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("map_questions_ibfk_2");
            });

            modelBuilder.Entity<MapUsers>(entity =>
            {
                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapUsers)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_users_ibfk_1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.MapUsers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("map_users_ibfk_2");
            });

            modelBuilder.Entity<MapVpdElements>(entity =>
            {
                entity.HasOne(d => d.Vpd)
                    .WithMany(p => p.MapVpdElements)
                    .HasForeignKey(d => d.VpdId)
                    .HasConstraintName("map_vpd_elements_ibfk_1");
            });

            modelBuilder.Entity<MapVpds>(entity =>
            {
                entity.HasOne(d => d.VpdType)
                    .WithMany(p => p.MapVpds)
                    .HasForeignKey(d => d.VpdTypeId)
                    .HasConstraintName("map_vpds_ibfk_1");
            });

            modelBuilder.Entity<Maps>(entity =>
            {
                entity.Property(e => e.IsTemplate).HasDefaultValueSql("'0'");

                entity.Property(e => e.Keywords).HasDefaultValueSql("''''''");

                entity.Property(e => e.LanguageId).HasDefaultValueSql("'1'");

                entity.Property(e => e.ReminderMsg).HasDefaultValueSql("''");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.Maps)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("maps_ibfk_7");

                entity.HasOne(d => d.Section)
                    .WithMany(p => p.Maps)
                    .HasForeignKey(d => d.SectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("maps_ibfk_6");

                entity.HasOne(d => d.Security)
                    .WithMany(p => p.Maps)
                    .HasForeignKey(d => d.SecurityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("maps_ibfk_2");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Maps)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("maps_ibfk_3");
            });

            modelBuilder.Entity<Options>(entity =>
            {
                entity.Property(e => e.Autoload).HasDefaultValueSql("'yes'");

                entity.Property(e => e.Name).HasDefaultValueSql("''");
            });

            modelBuilder.Entity<Phinxlog>(entity =>
            {
                entity.HasKey(e => e.Version)
                    .HasName("PRIMARY");

                entity.Property(e => e.Version).ValueGeneratedNever();

                entity.Property(e => e.EndTime).HasDefaultValueSql("'0000-00-00 00:00:00'");

                entity.Property(e => e.StartTime)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("current_timestamp()");
            });

            modelBuilder.Entity<QCumulative>(entity =>
            {
                entity.HasOne(d => d.Map)
                    .WithMany(p => p.QCumulative)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("q_cumulative_ibfk_2");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.QCumulative)
                    .HasForeignKey(d => d.QuestionId)
                    .HasConstraintName("q_cumulative_ibfk_1");
            });

            modelBuilder.Entity<ScenarioMaps>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.ScenarioMaps)
                    .HasForeignKey(d => d.MapId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_scenario_maps_maps");

                entity.HasOne(d => d.Scenario)
                    .WithMany(p => p.ScenarioMaps)
                    .HasForeignKey(d => d.ScenarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_scenario_maps_scenarios");
            });

            modelBuilder.Entity<Scenarios>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<SjtResponse>(entity =>
            {
                entity.HasOne(d => d.Response)
                    .WithMany(p => p.SjtResponse)
                    .HasForeignKey(d => d.ResponseId)
                    .HasConstraintName("sjt_response_ibfk_1");
            });

            modelBuilder.Entity<Statements>(entity =>
            {
                entity.Property(e => e.Initiator).HasDefaultValueSql("'1'");

                entity.Property(e => e.Timestamp).HasPrecision(18, 6);

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.Statements)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("statements_ibfk_1");
            });

            modelBuilder.Entity<StatisticsUserSessions>(entity =>
            {
                entity.Property(e => e.EndTime).HasPrecision(18, 6);

                entity.Property(e => e.StartTime).HasPrecision(18, 6);
            });

            modelBuilder.Entity<StatisticsUserSessionTraces>(entity =>
            {
                entity.Property(e => e.BookmarkMade).HasPrecision(18, 6);

                entity.Property(e => e.BookmarkUsed).HasPrecision(18, 6);

                entity.Property(e => e.DateStamp).HasPrecision(18, 6);
            });

            modelBuilder.Entity<SystemCounterActions>(entity =>
            {
                entity.HasOne(d => d.Counter)
                    .WithMany(p => p.SystemCounterActions)
                    .HasForeignKey(d => d.CounterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_counter_action_counter");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.SystemCounterActions)
                    .HasForeignKey(d => d.MapId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_counter_action_map");
            });

            modelBuilder.Entity<SystemCounters>(entity =>
            {
                entity.Property(e => e.Visible).HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<SystemFiles>(entity =>
            {
                entity.Property(e => e.HeightType).HasDefaultValueSql("'px'");

                entity.Property(e => e.IsShared).HasDefaultValueSql("'1'");

                entity.Property(e => e.IsSystem).HasDefaultValueSql("'0'");

                entity.Property(e => e.WidthType).HasDefaultValueSql("'px'");
            });

            modelBuilder.Entity<SystemQuestionResponses>(entity =>
            {
                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("system_question_responses_ibfk_2");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.SystemQuestionResponses)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("system_question_responses_ibfk_1");
            });

            modelBuilder.Entity<SystemQuestionValidation>(entity =>
            {
                entity.HasOne(d => d.Question)
                    .WithMany(p => p.SystemQuestionValidation)
                    .HasForeignKey(d => d.QuestionId)
                    .HasConstraintName("system_question_validation_ibfk_1");
            });

            modelBuilder.Entity<SystemQuestions>(entity =>
            {
                entity.Property(e => e.NumTries).HasDefaultValueSql("-1");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("system_questions_ibfk_2");
            });

            modelBuilder.Entity<SystemScripts>(entity =>
            {
                entity.Property(e => e.IsRaw).HasDefaultValueSql("b'0'");
            });

            modelBuilder.Entity<UserBookmarks>(entity =>
            {
                entity.HasOne(d => d.Node)
                    .WithMany(p => p.UserBookmarks)
                    .HasForeignKey(d => d.NodeId)
                    .HasConstraintName("user_bookmarks_ibfk_2");

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.UserBookmarks)
                    .HasForeignKey(d => d.SessionId)
                    .HasConstraintName("user_bookmarks_ibfk_1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserBookmarks)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("user_bookmarks_ibfk_3");
            });

            modelBuilder.Entity<UserGroups>(entity =>
            {
                entity.HasOne(d => d.Group)
                    .WithMany(p => p.UserGroups)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("user_groups_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserGroups)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("user_groups_ibfk_1");
            });

            modelBuilder.Entity<UserNotes>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasPrecision(18, 6);

                entity.Property(e => e.DeletedAt).HasPrecision(18, 6);

                entity.Property(e => e.UpdatedAt).HasPrecision(18, 6);

                entity.HasOne(d => d.Session)
                    .WithOne(p => p.UserNotes)
                    .HasForeignKey<UserNotes>(d => d.SessionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("user_notes_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserNotes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("user_notes_ibfk_1");

                entity.HasOne(d => d.Webinar)
                    .WithMany(p => p.UserNotes)
                    .HasForeignKey(d => d.WebinarId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("user_notes_ibfk_3");
            });

            modelBuilder.Entity<UserResponses>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasPrecision(18, 6);

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.UserResponses)
                    .HasForeignKey(d => d.QuestionId)
                    .HasConstraintName("user_responses_ibfk_1");

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.UserResponses)
                    .HasForeignKey(d => d.SessionId)
                    .HasConstraintName("user_responses_ibfk_2");
            });

            modelBuilder.Entity<UserSessions>(entity =>
            {
                entity.Property(e => e.EndTime).HasPrecision(18, 6);

                entity.Property(e => e.ResetAt).HasPrecision(18, 6);

                entity.Property(e => e.StartTime).HasPrecision(18, 6);

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.UserSessions)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("user_sessions_ibfk_2");
            });

            modelBuilder.Entity<UserSessionTraces>(entity =>
            {
                entity.Property(e => e.BookmarkMade).HasPrecision(18, 6);

                entity.Property(e => e.BookmarkUsed).HasPrecision(18, 6);

                entity.Property(e => e.DateStamp).HasPrecision(18, 6);

                entity.Property(e => e.EndDateStamp).HasPrecision(18, 6);

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.UserSessionTraces)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("user_sessiontraces_ibfk_3");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.UserSessionTraces)
                    .HasForeignKey(d => d.NodeId)
                    .HasConstraintName("user_sessiontraces_ibfk_6");

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.UserSessionTraces)
                    .HasForeignKey(d => d.SessionId)
                    .HasConstraintName("user_sessiontraces_ibfk_5");
            });

            modelBuilder.Entity<UserState>(entity =>
            {
                entity.HasOne(d => d.Map)
                    .WithMany(p => p.UserState)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("map_fk");

                entity.HasOne(d => d.MapNode)
                    .WithMany(p => p.UserState)
                    .HasForeignKey(d => d.MapNodeId)
                    .HasConstraintName("map_node_fk");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserState)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_fk");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.Property(e => e.IsLti).HasDefaultValueSql("'0'");

                entity.Property(e => e.VisualEditorAutosaveTime).HasDefaultValueSql("'50000'");
            });

            modelBuilder.Entity<WebinarMaps>(entity =>
            {
                entity.HasOne(d => d.StepNavigation)
                    .WithMany(p => p.WebinarMaps)
                    .HasForeignKey(d => d.Step)
                    .HasConstraintName("webinar_maps_ibfk_1");
            });

            modelBuilder.Entity<WebinarNodePoll>(entity =>
            {
                entity.HasOne(d => d.Node)
                    .WithMany(p => p.WebinarNodePoll)
                    .HasForeignKey(d => d.NodeId)
                    .HasConstraintName("webinar_node_poll_ibfk_2");

                entity.HasOne(d => d.Webinar)
                    .WithMany(p => p.WebinarNodePoll)
                    .HasForeignKey(d => d.WebinarId)
                    .HasConstraintName("webinar_node_poll_ibfk_1");
            });

            modelBuilder.Entity<WebinarPoll>(entity =>
            {
                entity.HasOne(d => d.OnNodeNavigation)
                    .WithMany(p => p.WebinarPollOnNodeNavigation)
                    .HasForeignKey(d => d.OnNode)
                    .HasConstraintName("webinar_poll_ibfk_1");

                entity.HasOne(d => d.ToNodeNavigation)
                    .WithMany(p => p.WebinarPollToNodeNavigation)
                    .HasForeignKey(d => d.ToNode)
                    .HasConstraintName("webinar_poll_ibfk_2");
            });

            modelBuilder.Entity<WebinarUsers>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.WebinarUsers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("webinar_users_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
