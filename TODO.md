# Lokpik TODO

## Next Up

## Current Backlog

- [ ] `BindAngle`: remap [0,1] to [0,90] with bindings at e.g. angles 10, 15, 18, 23, 29 deg
- [ ] min binding separation angle (the less the more difficult + drift is more manageable)

- [ ] **A1**: per-pin tension with 3 levels - need pin thickness? something else??
  - Free
  - Close [0,1]: 0 is slight contact, 1 is borderline binding
    - affects pin lift sensitivity/speed accordingly
  - Binding: pin CANNOT move unless plug tension is lowered

## Ideal Backlog

- [ ] attempting plug rotation asks every chamber. They can say "i'm blocked" or "i don't care" and also modify their internal state
- [ ] a "lift pick hard" button/repeated press to apply counter-rotation for unbinding a pin
- [ ] dynamic pick position with chamber walls
- [ ] look into FSM setup with `Lock` states and `Chamber` states
  - [ ] `Lock`: locked, picked, unlocked
  - [ ] `Chamber`: free, set, overset, underset, binding
- [ ] manipulating a pin usually also inadvertantly affects other pins due to the pick's shape

## Done

- [x] prevent pick movement when any pin lift is applied
- [x] wire up `Chamber.OnStateChange` to send rumble waves
  - [x] when a pin gets set (sudden quick rumble)
  - [x] when a pin gets unset (sudden quick rumble)
  - [x] while a pin is binding (continuous subtle rumble?)
