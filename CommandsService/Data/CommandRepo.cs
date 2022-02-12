using CommandsService.Models;

namespace CommandsService.Data;

public interface ICommandRepo
{
  bool SaveChanges();

  IEnumerable<Platform> GetAllPlatforms();
  void CreatePlatform(Platform platform);
  bool PlatformExistis(int platformId);
  bool ExternalPlatformExists(int externalPlatformId);

  IEnumerable<Command> GetCommandsForPlatform(int platformId);
  Command GetCommand(int platformId, int commandId);
  void CreateCommand(int platformId, Command command);

}

public class CommandRepo : ICommandRepo
{
  private readonly AppDbContext _context;

  public CommandRepo(AppDbContext context) => _context = context;

  public void CreateCommand(int platformId, Command command)
  {
    if (command is null)
      throw new ArgumentNullException(nameof(command));

    command.PlatformId = platformId;
    _context.Commands.Add(command);
  }

  public void CreatePlatform(Platform platform)
  {
    if (platform is null)
      throw new ArgumentNullException(nameof(platform));

    _context.Platforms.Add(platform);
  }

  public IEnumerable<Platform> GetAllPlatforms() => _context.Platforms.ToList();

  public Command GetCommand(int platformId, int commandId)
    => _context.Commands.FirstOrDefault(c => c.PlatformId == platformId && c.Id == commandId);

  public IEnumerable<Command> GetCommandsForPlatform(int platformId)
    => _context.Commands.Where(c => c.PlatformId == platformId).OrderBy(c => c.Platform.Name);

  public bool PlatformExistis(int platformId) => _context.Platforms.Any(p => p.Id == platformId);
  public bool ExternalPlatformExists(int externalPlatformId) => _context.Platforms.Any(p => p.Id == externalPlatformId);
  public bool SaveChanges() => (_context.SaveChanges() > 0);

}