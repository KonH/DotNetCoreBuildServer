using Server.BuildConfig;
using Server.Runtime;

namespace Server.Services {
	public interface IService {
		bool TryInit(BuildServer server, Project project);
	}
}