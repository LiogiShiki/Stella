#include "VertexArray.h"

namespace Lumen
{
	VertexArray::VertexArray()
	{
		this->array_id = 0;
		glGenVertexArrays(1, &(this->array_id));
		this->Bind();
	}

	VertexArray::~VertexArray()
	{
		glDeleteVertexArrays(1, &(this->array_id));
		this->Unbind();
	}

	void VertexArray::Bind() const 
	{
		glBindVertexArray(this->array_id);
	}

	void VertexArray::Unbind() const
	{
		glBindVertexArray(0);
	}
}