--- Coroutine that is called when the cutscene starts.
-- This invloves stuff like walking, jumping, displaying text boxes, etc.
function onBegin()
  disableMovement()
  wait(0.7)
  if getFlag("saw_theo") then
    say("bitspiecescollab_2_Intermediate_cumulus_heights_c1_umbrella_alt")
  else
    say("bitspiecescollab_2_Intermediate_cumulus_heights_c1_umbrella")
  end
end

--- Coroutine that is called when the talker is interacted with.
function onTalk() end

--- Callback for when the cutscene ends.
-- Function, no yielding actions allowed.
-- That means no walking, waiting etc.
-- Only "clean up" actions.
-- @tparam #Celeste.Level room Current room.
-- @bool wasSkipped If the cutscene was skipped.
function onEnd(room, wasSkipped)
  enableMovement()
end

--- Callback for when a player enters the trigger.
-- Only works as long as the cutscene is running.
-- @tparam #Celeste.Player player The player that entered the trigger.
function onEnter(player) end

--- Callback for when a player stays in the trigger (once per frame).
-- Only works as long as the cutscene is running.
-- @tparam #Celeste.Player player The player that is staying in the trigger.
function onStay(player) end

--- Callback for when a player leaves the trigger.
-- Only works as long as the cutscene is running.
-- @tparam #Celeste.Player player The player that exited the trigger.
function onLeave(player) end
