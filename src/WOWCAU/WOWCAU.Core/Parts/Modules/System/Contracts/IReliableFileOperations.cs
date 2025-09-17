﻿namespace WOWCAU.Core.Parts.Modules.System.Contracts
{
    public interface IReliableFileOperations
    {
        Task WaitAsync(CancellationToken cancellationToken = default);
        Task WaitBeforeAsync(Action fileOperations, CancellationToken cancellationToken = default);
        Task WaitAfterAsync(Action fileOperations, CancellationToken cancellationToken = default);
    }
}
