# Nearby Storage categories

Categories are grouped by broad family. Empty categories are hidden in the panel. Families and their categories are alphabetical, with **Miscellaneous** last.

## Category list

- **[Combat and Activities](#combat-and-activities)**
  - [Arrows](#arrows)
  - [Blob Bombs](#blob-bombs)
  - [Fireworks](#fireworks)
  - [Fishing](#fishing)
- **[Equipment](#equipment)**
  - [Accessories](#accessories)
  - [Armor](#armor)
  - [Clothing](#clothing)
  - [Shields](#shields)
  - [Tools](#tools)
  - [Weapons](#weapons)
- **[Food and Drinks](#food-and-drinks)**
  - [Cooked Food](#cooked-food)
  - [Cooked Meat](#cooked-meat)
  - [Feasts](#feasts)
  - [Potions](#potions)
  - [Raw Food](#raw-food)
  - [Raw Meat](#raw-meat)
- **[Resources](#resources)**
  - [Hides and Furs](#hides-and-furs)
  - [Ores and Metals](#ores-and-metals)
  - [Raw Materials](#raw-materials)
  - [Seeds](#seeds)
  - [Stone](#stone)
  - [Wood](#wood)
- **[Special Items](#special-items)**
  - [Trophies](#trophies)
  - [Valuables](#valuables)
  - [Miscellaneous](#miscellaneous)

## Automatic distribution priority

When `AutomaticItemDistribution` is enabled, Ctrl-click chooses nearby containers with free space in this order:

1. A container holding the same item, regardless of quality.
2. A renamed container whose name matches a word in the item's category.
3. A renamed container whose name matches a word in the item's broad family.
4. A container holding items in the same category; the highest item count wins.
5. A container holding items in the same broad family; the highest item count wins.
6. An empty container.
7. Any other container with free space.

Names match complete words longer than three letters, ignoring case and accents. Distance breaks remaining ties. If a stack does not fit in one container, distribution continues with the next eligible container.

## Combat and Activities

### Arrows

- Arrows, bolts, turret bolts, and snowballs.
- Other items that Valheim marks as ammunition.

### Blob Bombs

- Items whose prefab name begins with `Bomb`, including blob, ooze, bile, smoke, lava, siege, and dynamite bombs.
- Corked Vial.

### Fireworks

- Colored fireworks rockets: blue, cyan, green, purple, red, white, and yellow.
- Sparkler.

### Fishing

- Fishing Rod.
- Items Valheim marks as fish.
- Regular fishing bait and the bait variants for each biome.

## Equipment

### Accessories

- Utility items and trinkets.
- Dverger Circlet.

### Armor

- Helmets, chest pieces, leg pieces, hand pieces, and capes whose current armor value is not 1.

### Clothing

- Helmets, chest pieces, leg pieces, hand pieces, and capes showing exactly 1 armor point at their current quality.
- Linen Thread, Blue Jute, and Red Jute.

### Shields

- Shields.

### Tools

- Items Valheim marks as tools or torches.
- Pickaxes, one-handed axes, Scythe Handle, and Sharpening Stone.

### Weapons

- One-handed weapons other than axes, two-handed weapons, and bows.

## Food and Drinks

### Cooked Food

- Consumables produced by a recipe or cooking station, except food cooked over a fire and feasts.
- Serving Tray.

### Cooked Meat

- Items produced by cooking stations that require a fire.

### Feasts

- Feast food and its matching placement item.

### Potions

- Consumables Valheim marks as drinks, including meads and potions.
- Mead bases and barley wine base.

### Raw Food

- Consumables that are neither drinks nor prepared food.
- Turnip, bread dough, barley flour, and oat flour.

### Raw Meat

- Non-fish items used as inputs by cooking stations that require a fire.

## Resources

### Hides and Furs

- Deer hide, wolf pelt, lox pelt, serpent scale, troll hide, scale hide, Fenris hair, and bear hide.
- Leather scraps.

### Ores and Metals

- Copper, tin, bronze, iron, silver, black metal, flametal, and gold ores, scraps, or refined forms where available.
- Coal, chains, bronze nails, and iron nails.

### Raw Materials

- Material items that are not assigned to a more specific category, including crafted and processed materials.

### Seeds

- Carrot, turnip, onion, oat, kale, poteitr, beech, birch, ivy, and vineberry seeds.
- Fir and frost fir cones, pine cones, acorns, and ancient seeds.
- Barley and flax.

### Stone

- Stone, flint, and the companion Rock.

### Wood

- Wood, core wood, fine wood, ancient bark, and hard antler.
- Root and Writhan roots.

## Special Items

### Trophies

- Items Valheim marks as trophies.

### Valuables

- Items with a positive sell value, unless a more specific category takes priority.
- Surtling Core.

### Miscellaneous

- Other item types without a category above.
