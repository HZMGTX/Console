#!/usr/bin/env python3
"""Writes ServerData/ConsoleAssets.json and ConsoleAssets.txt from the bundles in ServerData.

Run it from the repository root after adding, replacing or removing a bundle:

    python3 Tools/make_asset_list.py

Every file in ServerData that starts with a Unity asset bundle header is listed. Its Unity
version is read from that header, so the menu can tell players which bundles their game
can open.
"""

import json
import math
import os
import sys

# Files kept beside the bundles that are never bundles themselves. Anything else is checked
# by its header, so a bundle with a dot in its name (console.main1) is still listed.
NOT_BUNDLES = (".png", ".jpg", ".jpeg", ".txt", ".json", ".md", ".ogg", ".wav", ".mp3")

SERVER_DATA = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "ServerData")


def read_cstring(data, start):
    end = data.index(b"\0", start)
    return data[start:end].decode("ascii", "replace"), end + 1


def unity_version(path):
    """The editor version a bundle was built with, or None if it isn't a UnityFS bundle."""
    with open(path, "rb") as handle:
        data = handle.read(128)
    try:
        signature, offset = read_cstring(data, 0)
        if signature != "UnityFS":
            return None
        offset += 4  # format version
        _, offset = read_cstring(data, offset)  # player version, e.g. 5.x.x
        version, _ = read_cstring(data, offset)
        return version or None
    except ValueError:
        return None


def main():
    bundles = []
    for name in sorted(os.listdir(SERVER_DATA), key=str.lower):
        path = os.path.join(SERVER_DATA, name)
        if not os.path.isfile(path) or name.lower().endswith(NOT_BUNDLES):
            continue

        version = unity_version(path)
        if version is None:
            print(f"skipped {name}: not a Unity asset bundle", file=sys.stderr)
            continue

        bundles.append({"name": name, "unity": version, "sizeKb": math.ceil(os.path.getsize(path) / 1024)})

    with open(os.path.join(SERVER_DATA, "ConsoleAssets.json"), "w", newline="\n") as handle:
        handle.write("[\n" + ",\n".join("  " + json.dumps(bundle) for bundle in bundles) + "\n]\n")

    with open(os.path.join(SERVER_DATA, "ConsoleAssets.txt"), "w", newline="\n") as handle:
        handle.write("".join(bundle["name"] + "\n" for bundle in bundles))

    print(f"Listed {len(bundles)} bundles.")


if __name__ == "__main__":
    main()
