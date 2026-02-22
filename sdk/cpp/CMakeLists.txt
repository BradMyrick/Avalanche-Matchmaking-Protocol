cmake_minimum_required(VERSION 3.10)
project(AmpCppSDK)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Find Cap'n Proto
find_package(CapnProto REQUIRED)
include_directories(${CAPNP_INCLUDE_DIRS} ./include ./src)
add_definitions(${CAPNP_DEFINITIONS})

# Add generated schema files
set(SCHEMA_SOURCES
    src/schemas/game_types.capnp.c++
    src/schemas/match.capnp.c++
    src/schemas/service.capnp.c++
)

# Add SDK implementation
set(SDK_SOURCES
    src/AmpClient.cpp
)

# Create a static library for the SDK
add_library(amp_sdk STATIC ${SCHEMA_SOURCES} ${SDK_SOURCES})
target_link_libraries(amp_sdk PUBLIC CapnProto::capnp-rpc CapnProto::capnp CapnProto::kj-async CapnProto::kj)
target_include_directories(amp_sdk PUBLIC include)

# Create an example/test executable
add_executable(amp_test src/main.cpp)
target_link_libraries(amp_test PRIVATE amp_sdk)
