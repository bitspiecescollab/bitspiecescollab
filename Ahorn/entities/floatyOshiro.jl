module BitsPieces.FloatyOshiro
  using ..Ahorn, Maple

  @mapdef Entity "BitsPieces/FloatyOshiro" FloatyOshiro(
    x::Integer, y::Integer,
    speed::Number=1.0,
    flag::String="",
    recolor::String="#FFFFFFFF",
    texture::String="oshiro_boss",
    fadeSpeed::Number=0.75,
    hitboxRadius::Number=14.0,
    framesPerParticle::Int=3,
    particleRadius::Number=10.0,
    particleColor1::String="#44B7FF",
    particleColor2::String="#75C9FF",
    wobbleSpeed::Number=2.0,
    wobbleAmplitude::Number=3.0,
    enabled::Bool=true,
    noLighting::Bool=false
  )

  const placements = Ahorn.PlacementDict(
    "Floaty Oshiro (Custom Oshiro)" => Ahorn.EntityPlacement(
      FloatyOshiro,
      "point",
      Dict{String, Any}(),
      function(entity)
        entity.data["nodes"] = [
          (Int(entity.data["x"]) - 10*4, Int(entity.data["y"]) + 0),
          (Int(entity.data["x"]) + 10*4, Int(entity.data["y"]) + 0)
        ]
      end
    )
  )

  Ahorn.nodeLimits(entity::FloatyOshiro) = 2, 2

  function Ahorn.selection(entity::FloatyOshiro)
    # sprite = strip(get(entity.data, "texture", "characters/oshiro"), '/') * "/boss13.png"
    sprite = "characters/oshiro/boss13.png"

    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)
    x0, y0 = Int.(nodes[1])
    x1, y1 = Int.(nodes[2])

    return [
      Ahorn.getSpriteRectangle(sprite, x, y),
      Ahorn.Rectangle(x0 - 5, y0 - 5, 10, 10),
      Ahorn.Rectangle(x1 - 5, y1 - 5, 10, 10)
    ]
  end

  function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FloatyOshiro, room::Maple.Room)
    # sprite = strip(get(entity.data, "texture", "characters/oshiro"), '/') * "/boss13.png"
    sprite = "characters/oshiro/boss13.png"

    Ahorn.drawSprite(ctx, sprite, 0, 0)

    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)
    x0, y0 = Int.(nodes[1])
    x1, y1 = Int.(nodes[2])

    Ahorn.drawArrow(ctx, x0 - x, y0 - y, x1 - x, y1 - y, Ahorn.colors.selection_selected_fc, headLength=4)
  end
end
