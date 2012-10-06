using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MyFirstGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        const float ROTATION_SENSITIVITY = 0.5f;
        const float SCROLL_SENSITIVITY = 0.005f;
        const float MIN_PITCH = -90;
        const float MAX_PITCH = 90;
        const float MIN_DIST = 5;
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        ObjModel model;

        VertexDeclaration vdecl; 
        VertexBuffer vb;
        IndexBuffer ib;

        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        BasicEffect basicEffect;
        RasterizerState normalState, wireframeState;


        float yaw = 0, pitch = 0;
        float dist = 10;
        
        bool dragging = false;
        int drag_x, drag_y;
        int scroll;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            worldMatrix = Matrix.Identity;
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
            
            float aspect = (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), aspect, 1.0f, 100.0f);

            basicEffect = new BasicEffect(graphics.GraphicsDevice);

            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;

            // primitive color
            basicEffect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            basicEffect.SpecularPower = 5.0f;
            basicEffect.Alpha = 1.0f;

            basicEffect.TextureEnabled = false;

            normalState = new RasterizerState();
            wireframeState = new RasterizerState();
            wireframeState.FillMode = FillMode.WireFrame;

            DepthStencilState dsState = new DepthStencilState();
            dsState.DepthBufferFunction = CompareFunction.LessEqual;

            GraphicsDevice.DepthStencilState = dsState;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            font = Content.Load<SpriteFont>("Arial");
            model = new ObjModel("model.obj");

            vdecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));

            vb = new VertexBuffer(GraphicsDevice, vdecl, model.Vertices.Length, BufferUsage.WriteOnly);
            vb.SetData(model.Vertices);

            ib = new IndexBuffer(GraphicsDevice, typeof(int), model.Indices.Length, BufferUsage.WriteOnly);
            ib.SetData(model.Indices);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && !dragging)
            {
                drag_x = mouseState.X;
                drag_y = mouseState.Y;
            }
            dragging = (mouseState.LeftButton == ButtonState.Pressed);

            if (dragging)
            {
                yaw += (float)(mouseState.X - drag_x) * 0.5f;
                pitch += (float)(mouseState.Y - drag_y) * 0.5f;
                drag_x = mouseState.X;
                drag_y = mouseState.Y;

                if (pitch > MAX_PITCH)
                    pitch = MAX_PITCH;
                if (pitch < MIN_PITCH)
                    pitch = MIN_PITCH;
            }

            dist -= (float)(mouseState.ScrollWheelValue - scroll) * 0.005f;
            scroll = mouseState.ScrollWheelValue;

            if (dist < MIN_DIST)
                dist = MIN_DIST;

            viewMatrix = Matrix.Identity;
            viewMatrix *= Matrix.CreateRotationY(MathHelper.ToRadians(yaw));
            viewMatrix *= Matrix.CreateRotationX(MathHelper.ToRadians(pitch));
            viewMatrix *= Matrix.CreateLookAt(new Vector3(0, 0, dist), Vector3.Zero, Vector3.Up); 
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkBlue);

            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;

            GraphicsDevice.RasterizerState = normalState;
            
            basicEffect.DiffuseColor = new Vector3(0.7f, 0.7f, 0.7f);
            GraphicsDevice.RasterizerState = normalState;
            drawModel();

            basicEffect.DiffuseColor = new Vector3(0.0f, 0.0f, 0.0f);
            GraphicsDevice.RasterizerState = wireframeState;
            //GraphicsDevice.DepthStencilState.DepthBufferFunction = CompareFunction.LessEqual;
            drawModel();

            base.Draw(gameTime);
        }

        private void drawModel()
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.Indices = ib;
                GraphicsDevice.SetVertexBuffer(vb);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, model.Vertices.Length, 0, model.Indices.Length / 3);
            }

        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            float aspect = (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
            basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), aspect, 1.0f, 100.0f);
        }
    }
}
