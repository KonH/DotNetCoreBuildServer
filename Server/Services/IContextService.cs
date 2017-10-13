using Server.Runtime;

namespace Server.Services {
	interface IContextService {
		RequestContext Context { get; }
	}
}
