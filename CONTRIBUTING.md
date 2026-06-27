# Modules and _Folders

## Modules (Content.FlagShip),

Modules are used to seperate our code from Upstreams Code (AKA Core), this not only eliminates the threat of merge conflicts but allows our code to be "decoupled" from core,

"Decoupled" simpily means that almost no code from core relies on something we made which is why the core modules cannot access our modules (but we can access theirs obv)

If you need to edit core at all you should always use comments to mark your exact changes, (see the below "commenting" section for more details),
If you need to communicate between Core and the FlagShip module, events should be put in Content.FlagShip.Common but this is rarely needed.

The use of modules also increases build time! Make sure to set up your IDE to run Content.FlagShip.Server and Content.FlagShip.Client instead of the default Core ones.

## _FlagShip Folders

In resouces there should only be _FlagShip, DO NOT ADD THE _FOLDER FROM THE FORK YOU ARE PORTING FROM, there is no reason to add theses folders as it only makes navigating resources annoying.

## Sounds

Sounds that are not directional, (Lobby music, antag intros, ect) can be stero,
All other sounds NEED to be mono.

## Commenting

Commenting is very important to do in upstream files examples of how it is done can be seen below:

### YAML COMMENTS

```yml

- type: entity
  id: BasePortal
  abstract: true
  name: FlagShip Cool Portal! # - FlagShip
  description: Transports you to a linked destination!
  components:
    # <FlagShip>
    - type: OurChangesGoHere
   # </FlagShip>
    - type: Transform
      anchored: false # - FlagShip
    - type: InteractionOutline
    - type: Clickable
    - type: Physics
      bodyType: Static
    - type: Sprite
      sprite: /Textures/Effects/portal.rsi
    - type: Fixtures
      fixtures:
      portalFixture:
      shape:
      !type:PhysShapeAabb
      bounds: "-0.25,-0.48,0.25,0.48"
      mask:
      - FullTileMask
      layer:
      - WallLayer
      hard: false
    - type: Portal
    - type: Gun
      fireRate: 15 # - FlagShip, was 8

```

Our Added Components are put at the top as any upstream changes will most likely add the components at the bottom
As you can see the comments for multi line changes are marked with xaml style brackets while 1 line changes are just a comment, note the firerate comment tells the orignal value of the varible, this is useful for quickly knowing without having to look at the pr that changed it

### C# COMMENTS

```csharp

public void ExampleCoreMethod()
{
    DoACoolThing();

    // <FlagShip>

        Our Code Goes here

    // </FlagShip>


    DoSomething(15, 18); - // FlagShip, was 12, 22
}

```

## Changelogs

Nothing really important just don't do rp stuff or vague stuff.
formatting below:

```yaml

- add: Added something.
- remove: Removed something.
- fix: Fixed something.
- tweak: Changed something.

```

## License

All Sounds in the game should _ideally_ have a creative commons or likewise license attached to it, but forks like CMU which blast copyrighted music 24/7 have shown that a cease and desist is unlikely.

### DO NOT TRY TO STEAL CODE FROM SERVERS THAT HAVE A NON COMBATIBLE LICENSE (They will come after us)
