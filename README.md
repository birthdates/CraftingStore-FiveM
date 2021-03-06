# CraftingStore for FiveM
This resource brings support for CraftingStore in FiveM to life. No need to worry about manually going through Patreons, and paying overpriced fees. I am not affiliated with CraftingStore in any way.

# How to use


1. Installing the resource

Download the resource https://github.com/birthdates/CraftingStore-FiveM/releases/download/1.0.1/FiveM.CraftingStore.rar.
Drag the resource into your FiveM server's resource folder.
Then add `start craftingstore` to your server.cfg.

2. Creating your CraftingStore

On CraftingStore if you do not see a FiveM option while creating your store, select Rust.

3. Setting up your CraftingStore API key

You can get your CraftingStore API key by creating a server on https://craftingstore.net/. Once you get that API you will put it your config.

4. Creating a package with commands

Once you've navigated through CraftingStore and learned how to create a package (which you can view at https://help.craftingstore.net/) you now need to setup events. These events will be automatically executed by the CraftingStore resource. You will want to put your events in the commands tab. Use {uuid} and not {player} so you get their Steam ID. (RECOMMENDED)

An example of an event would be `esx:giveMoney {uuid} 1000`
