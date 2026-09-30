# Spec Delta

## MODIFIED Requirements

### Requirement: Manual recruitment and persistent soldiers
A player SHALL recruit one soldier by explicitly activating one of their own barracks during an unpaused building phase while alive and not ready. The barracks SHALL offer Swordsman and Crossbowman recruitment explicitly. Each successful recruitment SHALL deduct the selected type's configured food cost, including the barracks upgrade reduction, and immediately add one soldier of that type to that city's army. Unknown soldier types SHALL be rejected atomically; requests without an explicit type SHALL select Swordsman. Barracks SHALL NOT automatically recruit during production, and gold SHALL NOT substitute for food. Living soldiers SHALL retain their identities, types and remaining health between waves; dead soldiers SHALL NOT return automatically.

#### Scenario: Click a barracks to recruit
- **WHEN** a player activates their barracks with enough food
- **THEN** the food cost is deducted once and one soldier is added
- **AND** passing a later production turn alone does not recruit another soldier

#### Scenario: Missing barracks or food
- **WHEN** a player requests recruitment from a non-barracks slot or without sufficient food
- **THEN** neither resources nor army state changes

#### Scenario: Army survives a battle
- **WHEN** a wave ends with some soldiers alive and some killed
- **THEN** only the surviving soldiers remain for the next building phase with their remaining health

#### Scenario: Recruit each weapon type
- **WHEN** a player with enough food explicitly recruits a Swordsman and a Crossbowman from their selected owned barracks
- **THEN** one unit of each selected type is created with its configured combat profile and each request spends its displayed food cost once
- **AND** the same barracks upgrade reduces both recruitment costs

#### Scenario: Unknown recruitment type
- **WHEN** a request contains an unknown soldier type or retries an already accepted typed recruitment
- **THEN** an unknown type changes no resources or army state and an accepted retry creates no additional soldier or charge

#### Scenario: Default recruitment remains melee
- **WHEN** an otherwise valid current-protocol recruitment request omits its soldier type
- **THEN** it creates one Swordsman for the configured melee cost


### Requirement: Automatic battles and city defense
Each city SHALL have an automatic battlefield with Swordsman and Crossbowman soldier types and one melee enemy type. Swordsmen and enemies SHALL approach melee range and Crossbowmen SHALL approach their longer shooting range and hold while a valid target remains in range. Each type SHALL use its own authoritative health, damage, speed, range, windup and recovery profile. Ranged hits SHALL be single-target attacks with no friendly fire; their cosmetic projectile SHALL NOT require physical collision or impart knockback. Soldiers SHALL fight without player orders. While alive, each city's built-in defender SHALL repeatedly attack living enemies assigned to that city at low damage even if its army has been wiped out. Enemies without a defending soldier SHALL attack city health. Army loss alone SHALL NOT eliminate the city. Building, recruitment, and upgrading SHALL be unavailable during combat. City health SHALL carry between waves without automatic healing.

#### Scenario: Exposed city fights back
- **WHEN** the last soldier dies while enemies remain
- **THEN** the city remains alive while its health is positive, enemies attack it, and its built-in defender continues damaging those enemies

#### Scenario: Remaining army needs no control
- **WHEN** a wave begins with soldiers present
- **THEN** they fight automatically and the server determines damage, deaths, and the battle result

#### Scenario: Ranged support fires behind melee contact
- **WHEN** a mixed army meets enemies with a valid shooting target beyond melee reach
- **THEN** Crossbowmen stop at shooting range and attack while Swordsmen advance to melee contact
- **AND** each hit resolves at its authoritative impact tick without requiring player orders or visual projectile collision

#### Scenario: Ranged soldier is reached
- **WHEN** an enemy reaches a Crossbowman after the melee screen is lost
- **THEN** the enemy can damage that soldier at melee range and the Crossbowman continues using its shooting profile without passing through the enemy

