﻿using DbMigrate.Model;
using DbMigrate.Model.Support.Database;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbMigrate.Tests.MigrateADatabase
{
    [TestClass]
    public class _1_PickTheGoal
    {
        [TestMethod]
        public void ShouldChooseToGoFromDatabaseCurrentVersionToTarget()
        {
            var database = new DatabaseLocalMemory();
            database.SetCurrentVersionTo(33);
            Target.FigureOutTheGoal(database, -9).ShouldHave()
                .Properties(spec => spec.CurrentVersion, spec => spec.TargetVersion)
                .EqualTo(new {CurrentVersion = 33, TargetVersion = -9});
        }
    }
}