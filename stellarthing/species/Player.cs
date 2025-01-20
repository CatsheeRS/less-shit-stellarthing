using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using starry;
using static starry.Starry;
namespace stellarthing;

/// <summary>
/// player
/// </summary>
public class Player : IEntity {
    public EntityType entityType => EntityType.GAME_WORLD;
    public string name => "Player";
    public string[] initGroups =>
        [Groups.PLAYER_GROUP, Groups.HUMAN_GROUP, Groups.SPECIES_GROUP];
    
    Tile? tile;
    Tile? lol;
    AnimationSprite? walkDown;
    AnimationSprite? walkUp;
    AnimationSprite? walkRight;
    AnimationSprite? walkLeft;
    TileParticles? lasparticulas;

    readonly double speed = 3.5;

    public async void create()
    {
        walkDown = new AnimationSprite(0.25,
            await load<Sprite>("species/bobdown1.png"),
            await load<Sprite>("species/bobdown2.png"),
            await load<Sprite>("species/bobdown3.png"),
            await load<Sprite>("species/bobdown4.png")
        );
        walkUp = new AnimationSprite(0.25,
            await load<Sprite>("species/bobup1.png"),
            await load<Sprite>("species/bobup2.png"),
            await load<Sprite>("species/bobup3.png"),
            await load<Sprite>("species/bobup4.png")
        );
        walkRight = new AnimationSprite(0.25,
            await load<Sprite>("species/bobright1.png"),
            await load<Sprite>("species/bobright2.png"),
            await load<Sprite>("species/bobright3.png"),
            await load<Sprite>("species/bobright4.png")
        );
        walkLeft = new AnimationSprite(0.25,
            await load<Sprite>("species/bobleft1.png"),
            await load<Sprite>("species/bobleft2.png"),
            await load<Sprite>("species/bobleft3.png"),
            await load<Sprite>("species/bobleft4.png")
        );
        
        tile = Entities.addComponent<Tile>(ent2ref(this));
        tile.sprite = new TileSprite(walkLeft, walkRight, walkUp, walkDown);
        
        lol = new() {
            sprite = new(await load<Sprite>("tiles/testl.png"),
                         await load<Sprite>("tiles/testr.png"),
                         await load<Sprite>("tiles/testt.png"),
                         await load<Sprite>("tiles/testb.png")),
            position = (1, 2, 0),
        };

        lasparticulas = new() {
            particle = await load<Sprite>("white.png"),
            amountFunc = () => (uint)StMath.randint(200, 6000),
            durationFunc = () => StMath.randfloat(1, 5),
            positionStartFunc = () => tile.position,
            positionEndFunc = () => StMath.randvec2((-10, -10), (10, 10)).as3d(tile.position.z),
            rotationStartFunc = () => 0,
            rotationEndFunc = () => StMath.randfloat(-360, 360),
            colorStartFunc = () => color.white,
            colorEndFunc = () => (255, 255, 255, 0),
        };

        var aaa = await load<Audio>("mrbeastification-killer-3000.wav");
        aaa.play();

        Client? mrpeepeepoopoo = null;
        async Task networkTest()
        {
            mrpeepeepoopoo = new() {
                username = "Mr Peepeepoopoo",
            };
            mrpeepeepoopoo.onDataReceived += (data, type) => log($"mr peepeepoopoo received {type}: {data}");
            Client johncrimes = new() {
                username = "John Crimes",
            };
            johncrimes.onDataReceived += (data, type) => log($"john crimes received {type}: {data}");
            Client mrbreast = new() {
                username = "MrBreast",
            };
            mrbreast.onDataReceived += (data, type) => log($"mrbreast received {type}: {data}");
            Client melonmusk = new() {
                username = "Melon Musk",
            };
            melonmusk.onDataReceived += (data, type) => log($"melon musk received {type}: {data}");

            Server.create(3, mrpeepeepoopoo);
            Server.connect("127.0.0.1", Server.GAME_PORT, mrpeepeepoopoo);
            Server.connect("127.0.0.1", Server.GAME_PORT, johncrimes);
            Server.connect("127.0.0.1", Server.GAME_PORT, mrbreast);
            Server.connect("127.0.0.1", Server.GAME_PORT, melonmusk);

            Server.onDataReceived += (data, type) => {
                if (type == "DO YOU LIKE BEANS?!?!??!!?") {
                    Server.sendToPlayer(JsonConvert.DeserializeObject<Client>(data)!.id, "yes.", "I DO LIKE BEANS");
                }
            };

            await Server.ask(johncrimes, johncrimes, "DO YOU LIKE BEANS?!?!??!!?");

            Server.onUpdate += delta => {
                log("server is being updated :)))");
            };

            Server.sendToAll("lol", "ATTENTION: BEANS");
            Server.sendToPlayer(mrbreast.id, "$5 billion dollars.", "give mem oney");
        }
        await networkTest();
        var timer = new Timer(5, false);
        timer.timeout += () => Server.cleanup(mrpeepeepoopoo!);
    }

    public async void update(double delta)
    {
        vec2i dir = (0, 0);
        // it's adding so you can move diagonally
        if (Input.isKeymapHeld("move_left")) dir += (-1, 0);
        if (Input.isKeymapHeld("move_right")) dir += (1, 0);
        if (Input.isKeymapHeld("move_up")) dir += (0, -1);
        if (Input.isKeymapHeld("move_down")) dir += (0, 1);

        // actually move
        tile!.position += (dir * (vec2)(speed, speed) * (vec2)(delta, delta)).as3d(tile.position.z);

        // animation stuff
        // it shouldn't go back to looking down when you didn't press anything
        if (dir != (0, 0)) {
            tile.side = dir switch {
                (1, 0) => TileSide.RIGHT,
                (-1, 0) => TileSide.LEFT,
                (0, 1) => TileSide.BOTTOM,
                (0, -1) => TileSide.TOP,
                _ => tile.side
            };

            // haha
            tile.sprite!.bottom = walkDown!;
            if (!walkDown!.playing) walkDown.start();
            if (!walkUp!.playing) walkUp.start();
            if (!walkLeft!.playing) walkLeft.start();
            if (!walkRight!.playing) walkRight.start();
        }
        else {
            // haha
            tile.sprite!.bottom = await load<Sprite>("species/bobdown0.png");
            walkDown!.stop();
            walkUp!.stop();
            walkLeft!.stop();
            walkRight!.stop();
            walkDown.currentFrame = 0;
            walkUp.currentFrame = 0;
            walkLeft.currentFrame = 0;
            walkRight.currentFrame = 0;
        }

        // the famous camera
        Tilemap.camPosition = tile.position.as2d();

        // why though
        if (Input.isKeyJustPressed(Key.SPACE)) {
            lasparticulas!.emit();
        }
    }

    public void draw()
    {
        Tilemap.pushTile(lol!);
        lasparticulas!.draw();
    }
}