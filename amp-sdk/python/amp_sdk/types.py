from dataclasses import dataclass
from typing import List, Optional

@dataclass
class PlayerProfileData:
    player_id: str
    display_name: str
    wallet_address: bytes
    global_mmr: float
    preferred_role: str
    region: str

@dataclass
class MatchRequest:
    game_id: str
    ruleset_id: str
    player_id: str
    mmr: float
    region: str

@dataclass
class MatchResult:
    match_id: str
    quality: float
    opponent_id: str
