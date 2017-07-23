using Server.BuildConfig;
using Server.Runtime;

namespace Server.Integrations {
	public interface IService {
		bool TryInit(BuildServer server, Project project);
	}
}