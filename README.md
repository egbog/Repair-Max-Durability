This repo is a mod for [sp-tarkov](https://sp-tarkov.com/)

# Repair Max Durability

Durability burn upon repair is a neglected feature in live EFT that only serves to artificially increase malfunction frequency. Aside from that, it has no other purpose or remedy.

This mod attempts to introduce a lore-friendly alternative to stripping attachments off of uselessly damaged weapons and vendoring the weapon, only to purchase the exact same weapon and put the same attachments on.

The simplest solution is to use the existing framework for weapon repairs and allow the repair to target the maximum durability instead. Lore-wise, a repair kit containing those crtical components that are then swapped out on the chosen weapon makes perfect sense and almost should be implemented in live EFT itself, albeit in a different fashion.

## Obtaining
Spare Firearm Parts can be crafted at Workbench lvl 1 using a Toolset (keep), 1 #FireKlean gun lube and 1 Weapon Parts.
They can also be purchased from Mechanic LL2 and Flea.

## Usage
Fully repair the weapon you want to fix using either traders or the vanilla Weapon Repair Kit. ie. (87.2/87.2)
Drag the Spare Firearm Parts onto your weapon. You should see a notification popup and hear a sound depending on if the repair was successful or a failure.

## Configuration
All parameters (price, trader, loyalty level etc.) are configurable in the /egbog-RepairMaxDurability/config/config.json file

It is possible to add multiple trades and multiple crafts, all with adjustable parameters.

### Multiple trades -

The default config includes two trades, one for Mechanic that is enabled and one for Prapor that is disabled. You can edit the Prapor trade directly and enable it or use it as a template to add more. Simply copy and paste the block, not forgetting to add a comma.

```json
"Traders": [
    {
      "Name": "Mechanic",
      "Enabled": true,
      "Price": 100000,
      "LoyaltyLevel": 2,
      "BuyLimit": 5,
      "Stock": 5
    },
    {
      "Name": "Prapor",
      "Enabled": false,
      "Price": 100000,
      "LoyaltyLevel": 1,
      "BuyLimit": 50,
      "Stock": 2000
    }
  ]
```


### Multiple crafts -

The default config only has one craft defined. ``Tool`` is an optional item that is required but not consumed during the craft. ``Item`` is the consumed ingredient and ``areaType: 10`` refers to the workbench. You can make another craft like so:

```json
"Crafts": [
   { 
      "default craft here..."
  },
  {
      "Enabled": true,
      "CraftTime": 69420,
      "AmountCrafted": 100,
      "Requirements": [
      {
        "type": "Item",
        "templateId": "your_mongoid_here",
        "isFunctional": false,
        "count": 10
      },
      {
        "type": "Area",
        "areaType": 10,
        "requiredLevel": 3
      }
    ]
  }
]
```

## Known Issues
None so far

## Compatibility 
Compatible with SPT 4.0.0
