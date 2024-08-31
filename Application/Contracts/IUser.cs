namespace Application.Contracts
{
	public interface IUser
	{
        Task<bool> CheckUserNameDuplicationAsync(string? userName);
        Task SoftDeleteUserAsync(int userId);
    }
}

