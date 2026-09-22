namespace MessageHub.Core.Entities;

public class UserProfile
{
  public Guid Id { get; private set; }
  public string Username { get; private set; }

  public string PasswordHash
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("PasswordHash must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  public string SecurityQuestion
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("SecurityQuestion must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  public string SecurityAnswerHash
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("SecurityAnswerHash must not be empty.", nameof(value));
      field = value;
    }
  } = null!;

  private UserProfile(Guid id, string username, string passwordHash, string securityQuestion, string securityAnswerHash)
  {
    Id = id;
    Username = username;
    PasswordHash = passwordHash;
    SecurityQuestion = securityQuestion;
    SecurityAnswerHash = securityAnswerHash;
  }

  /// <summary>
  /// Creates a new user profile. <paramref name="passwordHash"/> and <paramref name="securityAnswerHash"/>
  /// must already be hashed by the caller — hashing is an infrastructure concern, not a domain one.
  /// </summary>
  /// <exception cref="ArgumentException">Any required field is null or whitespace.</exception>
  public static UserProfile Create(string username, string passwordHash, string securityQuestion, string securityAnswerHash)
  {
    if (string.IsNullOrWhiteSpace(username))
      throw new ArgumentException("Username must not be empty.", nameof(username));

    return new UserProfile(Guid.NewGuid(), username, passwordHash, securityQuestion, securityAnswerHash);
  }

  /// <summary>
  /// Changes the password after the caller has already verified the current one.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="newPasswordHash"/> is null or whitespace.</exception>
  public void ChangePassword(string newPasswordHash)
  {
    PasswordHash = newPasswordHash;
  }

  /// <summary>
  /// Replaces the security question and answer, e.g. from account settings.
  /// </summary>
  /// <exception cref="ArgumentException">Any argument is null or whitespace.</exception>
  public void SetSecurityQuestion(string question, string answerHash)
  {
    SecurityQuestion = question;
    SecurityAnswerHash = answerHash;
  }

  /// <summary>
  /// Checks whether the given (already hashed) answer matches the stored security answer.
  /// </summary>
  public bool VerifySecurityAnswer(string answerHash)
  {
    return !string.IsNullOrWhiteSpace(answerHash) && SecurityAnswerHash == answerHash;
  }

  /// <summary>
  /// Resets the password after verifying the security answer, for use when the current password is unknown.
  /// </summary>
  /// <exception cref="InvalidOperationException"><paramref name="providedAnswerHash"/> does not match the stored answer.</exception>
  /// <exception cref="ArgumentException"><paramref name="newPasswordHash"/> is null or whitespace.</exception>
  public void ResetPassword(string providedAnswerHash, string newPasswordHash)
  {
    if (!VerifySecurityAnswer(providedAnswerHash))
      throw new InvalidOperationException("Security answer does not match.");

    ChangePassword(newPasswordHash);
  }
}
