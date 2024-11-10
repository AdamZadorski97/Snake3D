using NUnit.Framework;
using UnityEngine;

public class SnakeController3DTests
{
    private SnakeController3D snakeController;
    private GameObject snakeObject;

    [SetUp]
    public void SetUp()
    {
        // Create a new game object and attach the SnakeController3D script for testing
        snakeObject = new GameObject("Snake");
        snakeController = snakeObject.AddComponent<SnakeController3D>();

        // Manually set default values for testing if needed
        snakeController.moveSpeed = 2.0f;
        snakeController.rotationSpeed = 1.0f;
    }

    [Test]
    public void SnakeSpeedIsGreaterThanZero()
    {
        // Check that moveSpeed is greater than 0
        Assert.IsTrue(snakeController.moveSpeed > 0, $"moveSpeed should be greater than 0 but is {snakeController.moveSpeed}");
    }

    [Test]
    public void SnakeRotationSpeedIsGreaterThanZero()
    {
        // Check that rotationSpeed is greater than 0
        Assert.IsTrue(snakeController.rotationSpeed > 0, $"rotationSpeed should be greater than 0 but is {snakeController.rotationSpeed}");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the created game objects
        Object.DestroyImmediate(snakeObject);
    }
}
