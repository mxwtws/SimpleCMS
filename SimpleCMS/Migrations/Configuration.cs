namespace SimpleCMS.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using SimpleCMS.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SimpleCMS.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(SimpleCMS.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            #region 给表T_Category创建添加数据时触发器
            context.Database.ExecuteSqlCommand(@"
                IF OBJECT_ID(N'trg_CategoryInsert', N'TR') IS NOT NULL  
                    DROP TRIGGER trg_CategoryInsert;  
            ");

            context.Database.ExecuteSqlCommand(@"
                CREATE TRIGGER [dbo].[trg_CategoryInsert] 
                   ON  [dbo].[T_Category]
                   FOR INSERT
                AS 
                BEGIN

                    DECLARE @numrows int
                    SET @numrows = @@ROWCOUNT

                    if @numrows > 1
                    BEGIN
                        RAISERROR('只支持单行插入。', 16, 1)
                        ROLLBACK TRAN
                    END
                    ELSE
                    BEGIN
                        UPDATE
                            E
                        SET
                            HierarchyLevel    =
                            CASE
                                WHEN E.ParentId IS NULL THEN 0
                                ELSE Parent.HierarchyLevel + 1
                            END,
                            FullPath =
                            CASE
                                WHEN E.ParentId IS NULL THEN '.'
                                ELSE Parent.FullPath
                            END + CAST(E.Id AS nvarchar(10)) + '.'
                            FROM
                                T_Category AS E
                            INNER JOIN
                                inserted AS I ON I.Id = E.Id
                            LEFT OUTER JOIN
                                T_Category AS Parent ON Parent.Id = E.ParentId
                    END

                END
            ");
            #endregion

            #region 给表T_Category创建更新数据时触发器
            context.Database.ExecuteSqlCommand(@"
                IF OBJECT_ID(N'trg_CategoryUpdate', N'TR') IS NOT NULL  
                    DROP TRIGGER trg_CategoryUpdate;  
            ");

            context.Database.ExecuteSqlCommand(@"
                CREATE TRIGGER [dbo].[trg_CategoryUpdate]
                   ON  [dbo].[T_Category]
                   FOR Update
                AS 
                BEGIN
                  IF @@ROWCOUNT = 0
                        RETURN

                    if UPDATE(ParentId)
                    BEGIN
                        UPDATE
                            E
                        SET
                            HierarchyLevel    =
                                E.HierarchyLevel - I.HierarchyLevel +
                                    CASE
                                        WHEN I.ParentId IS NULL THEN 0
                                        ELSE Parent.HierarchyLevel + 1
                                    END,
                            FullPath =
                                ISNULL(Parent.FullPath, '.') +
                                CAST(I.Id as nvarchar(10)) + '.' +
                                RIGHT(E.FullPath, len(E.FullPath) - len(I.FullPath))
                            FROM
                                T_Category AS E
                            INNER JOIN
                                inserted AS I ON E.FullPath LIKE I.FullPath + '%'
                            LEFT OUTER JOIN
                                T_Category AS Parent ON I.ParentId = Parent.Id
                    END


                END
            ");
            #endregion

            #region 初始化数据
            // 初始化类别
            var category = context.Categories.SingleOrDefault(m => m.Title == "未分类");
            if (category == null)
            {
                context.Categories.Add(new Category()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "未分类",
                    Content = "未分类"
                });
            }
            else
            {
                category.Title = "未分类";
                category.Content = "未分类";
                category.ParentId = null;
            }
            context.SaveChanges();

            // 初始化角色
            context.Roles.AddOrUpdate(p => p.Name,
                new IdentityRole() { Name = "系统管理员" },
                new IdentityRole() { Name = "编辑" },
                new IdentityRole() { Name = "注册用户" }
            );

            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context)
            );

            var name = "admin";
            var user = userManager.FindByName(name);
            if (user != null) return;
            user = new ApplicationUser()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = name,
                Email = "admin@amdin.com",
                IsApprove = true,
                Created = DateTime.Now
            };
            userManager.Create(user, "123456");
            userManager.SetLockoutEnabled(user.Id, false);
            userManager.AddToRole(user.Id, "系统管理员");
            #endregion
        }
    }
}
