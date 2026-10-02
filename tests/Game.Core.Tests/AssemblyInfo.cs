using Xunit;

// Campaign concurrency is across isolated processes, not Arch world registries
// created and destroyed concurrently inside the same test process.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
