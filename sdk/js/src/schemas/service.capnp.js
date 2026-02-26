"use strict";
/* tslint:disable */
Object.defineProperty(exports, "__esModule", { value: true });
exports.MatchListener = exports.MatchSession = exports.UserSession = exports.GameSessionService = exports._capnpFileId = void 0;
const capnp_ts_1 = require("capnp-ts");
exports._capnpFileId = "a1b2c3d4e5f60003";
class GameSessionService extends capnp_ts_1.Struct {
    toString() { return "GameSessionService_" + super.toString(); }
}
exports.GameSessionService = GameSessionService;
GameSessionService._capnp = { displayName: "GameSessionService", id: "c24b6cc6fc418df9", size: new capnp_ts_1.ObjectSize(0, 0) };
class UserSession extends capnp_ts_1.Struct {
    toString() { return "UserSession_" + super.toString(); }
}
exports.UserSession = UserSession;
UserSession._capnp = { displayName: "UserSession", id: "ca30dd1a61881e14", size: new capnp_ts_1.ObjectSize(0, 0) };
class MatchSession extends capnp_ts_1.Struct {
    toString() { return "MatchSession_" + super.toString(); }
}
exports.MatchSession = MatchSession;
MatchSession._capnp = { displayName: "MatchSession", id: "8e8338034be7fdf9", size: new capnp_ts_1.ObjectSize(0, 0) };
class MatchListener extends capnp_ts_1.Struct {
    toString() { return "MatchListener_" + super.toString(); }
}
exports.MatchListener = MatchListener;
MatchListener._capnp = { displayName: "MatchListener", id: "fc81f34e1dd8cdcc", size: new capnp_ts_1.ObjectSize(0, 0) };
