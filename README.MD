# **MoreParts Mod**
_Add new parts and mechanics🚀_

## **About this mod**
MoreParts adds new parts and mechanics to the game. Next, each improvement that this mod adds will be detailed as a tutorial.

## **Colonies 👩‍🚀👨‍🚀**
`Colonies` are new buildings made up of different buildings that add benefits to the player and that you can create on any planet in the solar system (**except Earth**). 

### **Important Controls 🎮**
- `J`: Transfer resources from your rocket to the colony or from colony to the rocket. 
- `U`: Open colony Menu. 

### **Buildings List 📋**
To build each building, you will need to deposit resources by pressing the `J` key near the colony to deposit resources.

#### **1) Colony Base 🏠**

This building is the main base of the colonies and functions as an antenna for the `ARPANET` network.

**Construction Requirements ✔️**

- 7 Tons of `construction Material`
- 7 Tons of `electronic components`

**How to build 🔨**

When landing on a planet, the `Create colony` button will be enabled. Press the button and the resources of the rocket will be reduced and the `colony` will appear.

<img src="https://i.imgur.com/5MpB0XG.gif" width="320px">  

#### **2) Refinery ⛽**

The `refinery` will allow you to recharge your rocket fuel when you are near it (100mts) and process `Material`. This building needs electricity to work, so you will need to create `Solar Panels`.

**Construction Requirements ✔️**

- 12 Tons of `Construction Material` in the colony
- 10 tons of `electronic components` in the colony

**How to build 🔨**

You can create the `Refinery` on Colony Menu(Press `U`). Also, make sure the construction materials are in the colony and not in your rocket.   


#### **3) Solar Panels ☀️**
This building provides electricity to other buildings that need it to function

**Construction Requirements ✔️**
- 4 Tons of `construction material` in the colony
- 13 tons of `electronic components` in the colony

**How to build 🔨**

You can create the `Solar Panels` on Colony Menu.  


#### **4) VAB 🚧**
Allow you to create rockets in your colony.

**Construction Requirements ✔️**
- 10 Tons of `construction material` in the colony
- 4 tons of `electronic components` in the colony

**How to build 🔨**

You can create the `VAB` on Colony Menu.  


#### **5) Launch Pad 🚀**
Allow you to launch rockets from your colony, but the rocket must be created in your colony (you need to have the `VAB`).

**Construction Requirements ✔️**
- 20 Tons of `construction material` in the colony
- 1 Ton of `electronic components` in the colony

**How to build 🔨**

You can create the `Launch Pad` on Colony Menu. 


### **Colony Menu 📃**
To open the colony menu, you must be close to your colony and press `U` to display the following menu:  

![Imgur](https://i.imgur.com/4oxLoiAm.png)  

This menu allows you to create buildings in your colony and also view your `resources` and the name of the colony. The colony name is used to display your colony on the map as a landmark.  

![Imgur](https://i.imgur.com/SPNi6qBm.png)

You can also generate new resources from `Materials` collected from `Resource Deposit`. To do this, you just need to build a `refinery` and deposit `Material` in your colony. Once you have enough `Material`, you need to click on the resources you want to create in the `Colony Menu`.

![Imgur](https://i.imgur.com/qfrlppLm.png)

#### **Launch rockets from colony 🚀**  

When you enter the build screen, a colonies button should appear at the top right of your screen.  

![Imgur](https://i.imgur.com/7LKAL3qm.png)  

This button is used to select your launch pad. When you click on it, a menu will appear in the center of your screen with all your colonies (only colonies with `VAB` and `Launch Pad` built will appear).  

![Imgur](https://i.imgur.com/vtUJny4m.png)   

There is a restriction to launch your rocket from your colony. The restriction is that you **need** `Rocket Material` in your colony, because this material is used to build your rockets. To calculate the `Rocket Material` for your rockets, the mod takes the mass of the rocket and subtracts the mass of the fuel to get the dry mass of the rocket, this mass is used to find out if it is possible to launch your rocket. For example, if my dry mass is 10 tons (rocket without fuel) and my colony has 8 tons of `Rocket Material`, launch is not possible and will throw an error dialog. 

![Imgur](https://i.imgur.com/awXICSjm.png)  

**Note:** If you want to launch your rocket normally it is not necessary to select space center in the colony menu every time.  

### **Colony storage information 📁**

The information of your colonies is saved in the following path `Spaceflight Simulator Game\Saving\Worlds\(YOUR WORLD)\Persistent\colonies.json`. The file structure is this:
```json
[
  {
    "angle": 2.79255557,
    "position": {
      "x": -81448.966347933776,
      "y": 29642.345916831906
    },
    "name": "Colony Name",
    "address": "Moon",
    "Buildings": {
      "VAB": {
        "position": {
          "x": -81438.2385837931,
          "y": 29675.013171984618
        },
        "rotation": 2.792159
      },
      "Refinery": {
        "position": {
          "x": -81414.361418911489,
          "y": 29766.66173403928
        },
        "rotation": 2.79107141
      },
      "Solar Panel": {
        "position": {
          "x": -81428.901569678637,
          "y": 29727.196706625957
        },
        "rotation": 2.7915566
      }
    },
    "resources": {
      "Construction_Material": 3.0,
      "Electronic_Component": 2.0
    }
  }
]
```

## **Resource Deposits 🗻**

This is the new feature introduced in `v2.2.0`. Now the planets have `Resource Deposits`, this deposits are useful to extract `Material` resources. This resource is necessary to generate: `Electric Components`, `Construction and Rocket material` on `Refinery` building.

To found this `Resource Deposits` you will need to install in your satellite a `GeoEye` part. This part only can be activated in the space and not over 50 km where the atmosphere ends. For example, if the atmosphere ends at 30 km, you can only turn on the `GeoEye` between 30 km and 80 km.

![imgur](https://i.imgur.com/Vst9DOym.png)  

When a resource is discovered, it will show up in your map as landmark  

![imgur](https://i.imgur.com/J1V03kNm.jpg)  

To start extracting resources, you will need to create a rover with `Excavator` and `Material Cargo`. You will need to go to a `Resource Deposit` with the purpose of turn on the `Excavator`, otherwise you won't be able to turn on the part.

![imgur](https://i.imgur.com/y5IyXLWm.png)


## **ARPANET 📡**
The `ARPANET` is the name of the constellation of satellites that you can create to control your rockets. This game mechanic is optional and can only be activated by implementing the `Telecommunication dish` or `Antenna` part on your rocket.  

![Imgur](https://i.imgur.com/sMwKymEm.png)

### **Important Controls 🎮**
- `Y`: Turn On/Off the antenna
- `I`: Show or hide telecommunications lines

### **How To use 🤷**

Once the antenna is implemented in the rocket **it is necessary to activate the antenna**, to activate it press the letter **`Y`**. Once activated, the antenna will connect to the `space center antenna` and allow the rocket to be controlled. 

<img src="https://i.imgur.com/Hk9x9I0.gif"  width="320px">

Once the rocket is in orbit, it needs to have a connection to the space center antenna. This connection can occur directly or indirectly.
  
![Imgur](https://i.imgur.com/jnUThBrm.png)  
  
![Imgur](https://i.imgur.com/JIG6Bmdm.png)  

**Note:** To show or hide telecommunications lines press `I`.

## **Custom Parts 🆕**
Also some new parts are added to the game. Some of them are listed below:

- `Balloon`: This part adds a `balloon` to the game that allows you to lift objects without the need for motors. The thrust force of the balloon is calculated by the atmospheric density, so in an environment with low atmospheric density, the thrust force is less.  
![Imgur](https://i.imgur.com/2Ic9x4fm.jpg)  


- `Piston`: It is a simple piece that is used to push things.  
![Imgur](https://i.imgur.com/fbYxckvm.png)  

- `Electronic Cargo`: This part is used to transport Electronic components. It's used to create `colonies`.  
![Imgur](https://i.imgur.com/NHGLcoNm.png) 

- `Construction Cargo`: This part is used to transport Construction Material. it's used to create `colonies`.  
![Imgur](https://i.imgur.com/BLWyS4mm.png)

- `Parts cargo`: This part is used to transport rocket material. With this material you can assemble rockets in colonies.  
![Imgur](https://i.imgur.com/VBy8Rq3m.png)  

- `Material cargo`: This part is used to transport `material`. With this `material` you can generate rocket, construction and electronic materials in colonies.  
![Imgur](https://i.imgur.com/tp0benym.png)  

- `Rotor`: Add a rotor to the game that allows you to fly in the Game.  
![Imgur](https://i.imgur.com/x1qX1iJ.png)  

- `Continuous Track`: This part is like a wheel.
![Imgur](https://i.imgur.com/yqMNX8am.png)  

- `Excavator`: With this part you can extract resources from the `Resource Deposit`. You will need to add the `Material cargo` part to deposit the resource mined from the ground.  
![Imgur](https://i.imgur.com/EWaJ672m.png)  

- `GeoEye`: This is the planetary scanner. With this part you can discover a new `Resource Deposit`.  
![Imgur](https://i.imgur.com/eC3imYum.png)  


