using System.Runtime.CompilerServices;

// src
[assembly: InternalsVisibleTo("EntityDb.MongoDb")]
[assembly: InternalsVisibleTo("EntityDb.MongoDb.Provisioner")]
[assembly: InternalsVisibleTo("EntityDb.Mvc")]
[assembly: InternalsVisibleTo("EntityDb.Redis")]
[assembly: InternalsVisibleTo("EntityDb.Void")]

// test
[assembly: InternalsVisibleTo("EntityDb.Common.Tests")]
[assembly: InternalsVisibleTo("EntityDb.MongoDb.Tests")]
[assembly: InternalsVisibleTo("EntityDb.Mvc.Tests")]
[assembly: InternalsVisibleTo("EntityDb.Redis.Tests")]
