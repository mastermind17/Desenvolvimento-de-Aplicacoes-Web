import React from "react";

import ListComponent from "./ListComponent";
import FormComponent from "./FormComponent";

/**
* Este componente representa uma lista de tags para um dado projeto
* e ainda disponibiliza uma formulário para criação de novas tags.
*/
export default class TagWrapperComponent extends React.Component{

	render(){
		if (!this.props.tags) return null;
		return (
			<div>
				<h3>Set of Tags:</h3>
				<ListComponent
					list={this.props.tags.items}
					mapItems={(item, idx) => {
						return (<strong key={idx}>
											<a className="list-group-item">
												{item.data[0].value}
											</a>
										</strong>)
					}}
					/>
				<FormComponent
					class="tag"
					targetSubmitHref={this.props.tags.href}
					template={this.props.tags.template}
					onNewItemHandler={ (url, newItem) => this.props.onNewItemHandler(url, newItem, 'tags')}/>
			</div>);
	}


}
