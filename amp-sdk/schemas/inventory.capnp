@0xf46317ce822f0d09;

using Go = import "go_v3.capnp";
$Go.package("generated");
$Go.import("github.com/avalanche-matchmaking-protocol/amp-sdk/go/generated");

using Rust = import "rust.capnp";
$Rust.parentModule("inventory_capnp");

using Match = import "match.capnp";
using TimeStamp = Match.TimeStamp;
using Address = Match.Address;
using AmpId = Match.AmpId;
using Signature = Match.Signature;

# Item Rarity Levels
enum Rarity {
    common          @0;
    uncommon        @1;
    rare            @2;
    epic            @3;
    legendary       @4;
    mythic          @5;
    exclusive       @6;
}

# Item Types
enum ItemType {
    weapon          @0;
    armor           @1;
    consumable      @2;
    cosmetic        @3;
    ability         @4;
    perk            @5;
    currency        @6;
    material        @7;
    blueprint       @8;
    key             @9;
    container       @10;
}

# Item Metadata
struct ItemMetadata {
    name            @0 :Text;
    description     @1 :Text;
    iconUrl         @2 :Text;
    modelUrl        @3 :Text;        # 3D model URL
    animationUrl    @4 :Text;        # Animation URL
    soundUrl        @5 :Text;        # Sound effect URL
    
    # Gameplay Properties
    rarity          @6 :Rarity;
    itemType        @7 :ItemType;
    slot            @8 :Text;        # Equipment slot
    levelRequirement @9 :UInt32;
    classRequirement @10 :Text;      # Character class requirement
    factionRequirement @11 :Text;    # Faction requirement
    
    # Stats
    baseStats       @12 :Data;       # Serialized stat block
    scalingStats    @13 :Data;       # Level-based scaling
    enchantmentSlots @14 :UInt8;     # Number of enchantment slots
    
    # Durability & Usage
    maxDurability   @15 :UInt32;
    maxStack        @16 :UInt32;     # Maximum stack size
    isConsumable    @17 :Bool;
    cooldownMs      @18 :UInt64;     # Usage cooldown
    
    # Economic Properties
    isTradable      @19 :Bool;
    isSoulbound     @20 :Bool;       # Bound to player
    isDestroyable   @21 :Bool;
    baseValue       @22 :UInt64;     # Base monetary value
    
    # On-Chain Properties
    tokenStandard   @23 :TokenStandard;
    tokenId         @24 :UInt256;    # ERC-721/1155 token ID
    contractAddress @25 :Address;    # Smart contract address
    chainId         @26 :UInt64;     # Blockchain ID
    
    # Metadata Extensions
    tags            @27 :List(Text);
    creationDate    @28 :TimeStamp;
    lastUpdated     @29 :TimeStamp;
}

using UInt256 = Data; # 32-byte unsigned integer

enum TokenStandard {
    none            @0;  # Off-chain item
    erc20           @1;  # Fungible token
    erc721          @2;  # Non-fungible token
    erc1155         @3;  # Semi-fungible token
    avalancheNative @4;  # Avalanche native token
}

# Inventory Item Instance
struct InventoryItem {
    itemId          @0 :AmpId;       # Unique instance ID
    templateId      @1 :AmpId;       # Reference to ItemMetadata
    ownerId         @2 :AmpId;       # Player ID
    
    # Instance Properties
    quantity        @3 :UInt32;      # For stackable items
    durability      @4 :UInt32;      # Current durability
    level           @5 :UInt32;      # Item level
    experience      @6 :UInt64;      # Item XP
    
    # Modifications
    enchantments    @7 :List(Enchantment);
    sockets         @8 :List(Socket);
    customName      @9 :Text;
    
    # State
    isEquipped      @10 :Bool;
    equippedSlot    @11 :Text;
    isBound         @12 :Bool;
    isLocked        @13 :Bool;       # Cannot be traded/moved
    
    # Transaction History
    acquiredAt      @14 :TimeStamp;
    lastUsed        @15 :TimeStamp;
    transactionHash @16 :Data;       # Blockchain transaction
}

struct Enchantment {
    enchantmentId   @0 :AmpId;
    name            @1 :Text;
    effect          @2 :Data;        # Serialized effect data
    power           @3 :UInt32;
    duration        @4 :UInt64;      # 0 = permanent
}

struct Socket {
    socketType      @0 :Text;        # "gem", "rune", "mod"
    gemId           @1 :AmpId;       # Installed gem/item
    isActive        @2 :Bool;
}

# Loadout/Equipment Set
struct Loadout {
    loadoutId       @0 :AmpId;
    name            @1 :Text;
    playerId        @2 :AmpId;
    
    # Equipment Slots
    slotMappings    @3 :List(SlotMapping);
    
    # Loadout Properties
    isActive        @4 :Bool;
    isDefault       @5 :Bool;
    gameId          @6 :AmpId;       # Game-specific loadout
    modeId          @7 :AmpId;       # Mode-specific loadout
    
    # Stats Summary
    totalStats      @8 :Data;        # Serialized combined stats
    rating          @9 :UInt32;      # Loadout power rating
    
    # Metadata
    createdAt       @10 :TimeStamp;
    lastUsed        @11 :TimeStamp;
    usageCount      @12 :UInt32;
}

struct SlotMapping {
    slotType        @0 :Text;        # "weapon", "armor", "ability", etc.
    itemId          @1 :AmpId;       # InventoryItem ID
    position        @2 :UInt8;       # Slot position (for multiple of same type)
}

# Crafting & Recipes
struct Recipe {
    recipeId        @0 :AmpId;
    name            @1 :Text;
    description     @2 :Text;
    
    # Ingredients
    ingredients     @3 :List(RecipeIngredient);
    
    # Output
    outputItem      @4 :AmpId;       # ItemMetadata template ID
    outputQuantity  @5 :UInt32;
    successRate     @6 :Float32;     # 0.0-1.0
    
    # Requirements
    skillLevel      @7 :UInt32;
    workstation     @8 :Text;        # Required workstation
    craftingTimeMs  @9 :UInt64;
    
    # Economic Properties
    isDiscoverable  @10 :Bool;
    isTradable      @11 :Bool;
}

struct RecipeIngredient {
    itemId          @0 :AmpId;       # ItemMetadata template ID
    quantity        @1 :UInt32;
    quality         @2 :Rarity;      # Minimum rarity required
    isConsumed      @3 :Bool;
}

# Marketplace & Trading
struct TradeOffer {
    offerId         @0 :AmpId;
    sellerId        @1 :AmpId;
    
    # Items for trade
    offeredItems    @2 :List(TradeItem);
    requestedItems  @3 :List(TradeItem);
    
    # Currency
    price           @4 :UInt64;
    currencyToken   @5 :Address;     # ERC-20 token address
    
    # Trade Conditions
    expiration      @6 :TimeStamp;
    isActive        @7 :Bool;
    isAuction       @8 :Bool;
    minimumBid      @9 :UInt64;      # For auctions
    buyoutPrice     @10 :UInt64;     # Instant buy price
    
    # Blockchain Integration
    escrowAddress   @11 :Address;    # Escrow smart contract
    chainId         @12 :UInt64;
}

struct TradeItem {
    itemId          @0 :AmpId;       # InventoryItem ID
    quantity        @1 :UInt32;
    inspectionHash  @2 :Data;        # Hash of item properties for verification
}

# Service Interfaces
interface InventoryService {
    # Item Management
    getItemMetadata @0 (itemId :AmpId) -> (metadata :ItemMetadata);
    createItem @1 (metadata :ItemMetadata, signature :Signature) -> (itemId :AmpId);
    updateItem @2 (itemId :AmpId, metadata :ItemMetadata, signature :Signature) -> ();
    
    # Player Inventory
    getInventory @3 (playerId :AmpId) -> (items :List(InventoryItem));
    addItem @4 (playerId :AmpId, templateId :AmpId, quantity :UInt32, signature :Signature) -> (itemId :AmpId);
    removeItem @5 (playerId :AmpId, itemId :AmpId, quantity :UInt32, signature :Signature) -> ();
    transferItem @6 (fromPlayer :AmpId, toPlayer :AmpId, itemId :AmpId, quantity :UInt32, signature :Signature) -> ();
    
    # Equipment & Loadouts
    equipItem @7 (playerId :AmpId, itemId :AmpId, slot :Text) -> ();
    unequipItem @8 (playerId :AmpId, slot :Text) -> ();
    createLoadout @9 (loadout :Loadout, signature :Signature) -> (loadoutId :AmpId);
    activateLoadout @10 (playerId :AmpId, loadoutId :AmpId) -> ();
    
    # Crafting
    craftItem @11 (playerId :AmpId, recipeId :AmpId, signature :Signature) -> (success :Bool, itemId :AmpId);
    getRecipes @12 (playerId :AmpId, filter :RecipeFilter) -> (recipes :List(Recipe));
    
    # Marketplace
    createTradeOffer @13 (offer :TradeOffer, signature :Signature) -> (offerId :AmpId);
    acceptTradeOffer @14 (offerId :AmpId, buyerId :AmpId, signature :Signature) -> ();
    cancelTradeOffer @15 (offerId :AmpId, signature :Signature) -> ();
    
    # Analytics
    getInventoryValue @16 (playerId :AmpId) -> (value :UInt64, breakdown :List(AssetValue));
}

struct RecipeFilter {
    requiredIngredients @0 :List(AmpId);
    maxDifficulty   @1 :UInt32;
    availableOnly   @2 :Bool;        # Recipes player can craft now
}

struct AssetValue {
    itemId          @0 :AmpId;
    quantity        @1 :UInt32;
    unitValue       @2 :UInt64;
    totalValue      @3 :UInt64;
    valuationMethod @4 :Text;        # "market", "crafting", "base"
}